"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { transactionService } from "@/services/transaction-service";
import { ApiError } from "@/lib/api";

const schema = z.object({
  type: z.enum(["Payment", "Transfer"]),
  amount: z
    .number({ error: "Ingresa un monto valido" })
    .positive("Debe ser mayor a 0"),
  description: z.string().min(1, "La descripcion es requerida"),
});

type Fields = z.infer<typeof schema>;

export default function CreateTransactionForm() {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<Fields>({
    resolver: zodResolver(schema),
    defaultValues: { type: "Payment" },
  });

  async function onSubmit(data: Fields) {
    setApiError(null);
    try {
      await transactionService.create({
        idempotencyKey: crypto.randomUUID(),
        type: data.type,
        amount: data.amount,
        description: data.description,
      });
      reset();
      setOpen(false);
      router.refresh();
    } catch (err) {
      setApiError(err instanceof ApiError ? err.message : "Error al crear la transaccion");
    }
  }

  if (!open) {
    return (
      <button
        onClick={() => setOpen(true)}
        className="bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold px-4 py-2 rounded-lg transition-colors"
      >
        + Nueva transaccion
      </button>
    );
  }

  return (
    <form
      onSubmit={handleSubmit(onSubmit)}
      className="bg-white rounded-xl border border-gray-200 p-5 space-y-4 mb-6"
    >
      <div className="flex items-center justify-between">
        <h2 className="font-semibold text-gray-800">Nueva transaccion de prueba</h2>
        <button
          type="button"
          onClick={() => { setOpen(false); reset(); setApiError(null); }}
          className="text-gray-400 hover:text-gray-600 text-sm"
        >
          Cancelar
        </button>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="flex flex-col gap-1">
          <label className="text-sm text-gray-600">Tipo</label>
          <select
            {...register("type")}
            className="border border-gray-300 rounded-lg px-3 py-2 text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-400"
          >
            <option value="Payment">Pago</option>
            <option value="Transfer">Transferencia</option>
          </select>
        </div>

        <div className="flex flex-col gap-1">
          <label className="text-sm text-gray-600">Monto ($)</label>
          <input
            type="number"
            step="0.01"
            placeholder="100"
            {...register("amount", { valueAsNumber: true })}
            className={`border rounded-lg px-3 py-2 text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-400 ${
              errors.amount ? "border-red-400 bg-red-50" : "border-gray-300"
            }`}
          />
          {errors.amount && (
            <span className="text-xs text-red-500">{errors.amount.message}</span>
          )}
        </div>

        <div className="flex flex-col gap-1">
          <label className="text-sm text-gray-600">Descripcion</label>
          <input
            type="text"
            placeholder="Pago de cuota"
            {...register("description")}
            className={`border rounded-lg px-3 py-2 text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-400 ${
              errors.description ? "border-red-400 bg-red-50" : "border-gray-300"
            }`}
          />
          {errors.description && (
            <span className="text-xs text-red-500">{errors.description.message}</span>
          )}
        </div>
      </div>

      {apiError && (
        <div className="bg-red-50 border border-red-200 text-red-700 rounded-lg px-4 py-3 text-sm">
          {apiError}
        </div>
      )}

      <button
        type="submit"
        disabled={isSubmitting}
        className="bg-green-600 hover:bg-green-700 disabled:opacity-50 text-white font-semibold px-5 py-2 rounded-lg text-sm transition-colors"
      >
        {isSubmitting ? "Enviando..." : "Crear transaccion"}
      </button>
    </form>
  );
}
