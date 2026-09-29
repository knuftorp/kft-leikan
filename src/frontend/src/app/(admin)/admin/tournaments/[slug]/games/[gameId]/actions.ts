"use server";
import { revalidatePath } from "next/cache";

// API-basis-URL — hentes fra miljøvariabel eller faller tilbake til localhost
const API_BASE = process.env.API_BASE_URL ?? "http://localhost:5000";

// Legger til en deltaker i et spill via POST /api/v1/games/:gameId/participants
export async function addParticipantAction(
  gameId: string,
  personId: string,
  tournamentSlug: string
) {
  await fetch(`${API_BASE}/api/v1/games/${gameId}/participants`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ personId }),
  });
  revalidatePath(`/admin/tournaments/${tournamentSlug}/games/${gameId}`);
}

// Tilstand for arrangørskjemaet - error vises i skjemaet
export type AddOrganizerState = { error?: string };

// Legger til en arrangør med rolle via POST /api/v1/games/:gameId/organizers.
// Er personen allerede arrangør, erstattes rollen.
export async function addOrganizerAction(
  gameId: string,
  tournamentSlug: string,
  _prevState: AddOrganizerState,
  formData: FormData
): Promise<AddOrganizerState> {
  const personId = formData.get("personId");
  const role = formData.get("role");

  // Validerer på serveren - required i skjemaet beskytter bare i nettleseren
  if (typeof personId !== "string" || personId === "") {
    return { error: "Velg en spiller." };
  }
  if (role !== "spilte" && role !== "dømte") {
    return { error: "Velg om arrangøren spilte eller dømte." };
  }

  let res: Response;
  try {
    res = await fetch(`${API_BASE}/api/v1/games/${gameId}/organizers`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ personId, withParticipation: role === "spilte" }),
    });
  } catch {
    return { error: "Fikk ikke kontakt med API-et. Prøv igjen om litt." };
  }

  if (!res.ok) {
    // API-et svarer med ProblemDetails - viser detail hvis den finnes
    const problem = await res.json().catch(() => null);
    return { error: problem?.detail ?? "Kunne ikke legge til arrangør." };
  }

  revalidatePath(`/admin/tournaments/${tournamentSlug}/games/${gameId}`);
  return {};
}

// Fullfører et spill med plasseringer via POST /api/v1/games/:gameId/complete
export async function completeGameAction(
  gameId: string,
  tournamentSlug: string,
  formData: FormData
) {
  const firstPlace = formData.getAll("firstPlace") as string[];
  const secondPlace = formData.getAll("secondPlace") as string[];
  const thirdPlace = formData.getAll("thirdPlace") as string[];

  const res = await fetch(`${API_BASE}/api/v1/games/${gameId}/complete`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ gameId, firstPlace, secondPlace, thirdPlace }),
  });

  if (!res.ok) throw new Error("Kunne ikke fullføre spill");
  revalidatePath(`/admin/tournaments/${tournamentSlug}/games/${gameId}`);
}
