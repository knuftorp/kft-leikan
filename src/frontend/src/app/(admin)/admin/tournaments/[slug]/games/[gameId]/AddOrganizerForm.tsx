"use client";

import { useActionState } from "react";
import type { AddOrganizerState } from "./actions";

type Person = {
  id: string;
  firstName: string;
  lastName: string;
};

type Props = {
  persons: Person[];
  action: (
    prevState: AddOrganizerState,
    formData: FormData
  ) => Promise<AddOrganizerState>;
};

// Skjema for å legge til arrangør med rolle - klientkomponent for å vise feil fra Server Action
export default function AddOrganizerForm({ persons, action }: Props) {
  const [state, formAction, isPending] = useActionState(action, {});

  return (
    <form action={formAction} className="flex flex-wrap items-end gap-3">
      <div className="flex-1 min-w-[220px]">
        <label
          htmlFor="organizerPersonId"
          className="block text-sm font-medium mb-1"
        >
          Spiller
        </label>
        <select
          id="organizerPersonId"
          name="personId"
          className="rounded border border-gray-300 px-2 py-1.5 text-sm w-full"
          required
        >
          <option value="">Velg spiller…</option>
          {persons.map((person) => (
            <option key={person.id} value={person.id}>
              {person.lastName}, {person.firstName}
            </option>
          ))}
        </select>
      </div>

      <fieldset className="flex items-center gap-4 py-1.5">
        <legend className="sr-only">Rolle</legend>
        <label className="flex items-center gap-2 text-sm">
          <input type="radio" name="role" value="spilte" required />
          Spilte
        </label>
        <label className="flex items-center gap-2 text-sm">
          <input type="radio" name="role" value="dømte" />
          Dømte
        </label>
      </fieldset>

      <button
        type="submit"
        disabled={isPending}
        className="rounded bg-gray-900 px-3 py-1.5 text-sm text-white hover:bg-gray-700 disabled:opacity-50"
      >
        Legg til
      </button>

      {state.error && (
        <p className="w-full text-sm text-red-600" role="alert">
          {state.error}
        </p>
      )}
    </form>
  );
}
