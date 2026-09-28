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

// Legger til en arrangør med rolle via POST /api/v1/games/:gameId/organizers.
// Er personen allerede arrangør, erstattes rollen.
export async function addOrganizerAction(
  gameId: string,
  tournamentSlug: string,
  formData: FormData
) {
  const personId = formData.get("personId") as string;
  const withParticipation = formData.get("role") === "spilte";

  const res = await fetch(`${API_BASE}/api/v1/games/${gameId}/organizers`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ personId, withParticipation }),
  });

  if (!res.ok) throw new Error("Kunne ikke legge til arrangør");
  revalidatePath(`/admin/tournaments/${tournamentSlug}/games/${gameId}`);
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
