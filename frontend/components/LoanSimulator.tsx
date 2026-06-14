"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { loanService } from "@/services/loan-service";
import { SimulateLoanResponse } from "@/types/loan";
import PaymentScheduleTable from "./PaymentScheduleTable";
import { ApiError } from "@/lib/api";

const simulatorSchema = z.object({
  amount: z
    .number({ error: "Ingresa un monto válido" })
    .min(500, "Monto mínimo: $500")
    .max(50000, "Monto máximo: $50,000"),
  termMonths: z
    .number({ error: "Ingresa un plazo válido" })
    .int("El plazo debe ser un número entero")
    .min(6, "Plazo mínimo: 6 meses")
    .max(60, "Plazo máximo: 60 meses"),
  monthlyIncome: z
    .number({ error: "Ingresa un ingreso válido" })
    .positive("Debe ser mayor a 0")
    .optional(),
});

type SimulatorFields = z.infer<typeof simulatorSchema>;

export default function LoanSimulator() {
  const [result, setResult] = useState<SimulateLoanResponse | null>(null);
  const [apiError, setApiError] = useState<string | null>(null);
  const [applying, setApplying] = useState(false);
  const [applied, setApplied] = useState(false);

  const {
    register,
    handleSubmit,
    getValues,
    formState: { errors, isSubmitting },
  } = useForm<SimulatorFields>({
    resolver: zodResolver(simulatorSchema),
  });

  async function onSimulate(data: SimulatorFields) {
    setApiError(null);
    setResult(null);
    setApplied(false);
    try {
      const res = await loanService.simulate({
        amount: data.amount,
        termMonths: data.termMonths,
        loanType: "Fixed",
      });
      setResult(res);
    } catch (err) {
      setApiError(err instanceof ApiError ? err.message : "Error al simular");
    }
  }

  async function handleApply() {
    if (!result) return;
    const { amount, termMonths, monthlyIncome } = getValues();
    setApplying(true);
    setApiError(null);
    try {
      await loanService.apply({
        amount,
        termMonths,
        loanType: "Fixed",
        monthlyIncome,
        userId: "user-hardcoded-001",
      });
      setApplied(true);
    } catch (err) {
      setApiError(err instanceof ApiError ? err.message : "Error al solicitar");
    } finally {
      setApplying(false);
    }
  }

  return (
    <div className="space-y-8">
      <form
        onSubmit={handleSubmit(onSimulate)}
        className="bg-white rounded-xl border border-gray-200 p-6 space-y-4"
      >
        <h2 className="font-semibold text-gray-800 text-lg">Parámetros</h2>

        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <Field label="Monto ($)" error={errors.amount?.message}>
            <input
              type="number"
              step="0.01"
              placeholder="10000"
              {...register("amount", { valueAsNumber: true })}
              className={inputClass(!!errors.amount)}
            />
          </Field>

          <Field label="Plazo (meses)" error={errors.termMonths?.message}>
            <input
              type="number"
              placeholder="12"
              {...register("termMonths", { valueAsNumber: true })}
              className={inputClass(!!errors.termMonths)}
            />
          </Field>

          <Field
            label="Ingreso mensual ($)"
            hint="Solo para solicitar"
            error={errors.monthlyIncome?.message}
          >
            <input
              type="number"
              step="0.01"
              placeholder="Opcional"
              {...register("monthlyIncome", { setValueAs: (v) => v === "" ? undefined : Number(v) })}
              className={inputClass(!!errors.monthlyIncome)}
            />
          </Field>
        </div>

        <button
          type="submit"
          disabled={isSubmitting}
          className="bg-blue-600 hover:bg-blue-700 disabled:opacity-50 text-white font-semibold px-6 py-2 rounded-lg text-sm transition-colors"
        >
          {isSubmitting ? "Calculando..." : "Simular"}
        </button>
      </form>

      {apiError && (
        <div className="bg-red-50 border border-red-200 text-red-700 rounded-lg px-4 py-3 text-sm">
          {apiError}
        </div>
      )}

      {result && (
        <div className="space-y-6">
          <div className="bg-white rounded-xl border border-gray-200 p-6">
            <h2 className="font-semibold text-gray-800 text-lg mb-4">Resumen</h2>
            {(() => {
              const totalPayment = result.schedule.reduce((s, r) => s + r.totalPayment, 0);
              const totalInterest = result.schedule.reduce((s, r) => s + r.interest, 0);
              return (
                <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
                  <Stat label="Cuota mensual" value={`$${result.monthlyPayment.toFixed(2)}`} highlight />
                  <Stat label="Total a pagar" value={`$${totalPayment.toFixed(2)}`} />
                  <Stat label="Total intereses" value={`$${totalInterest.toFixed(2)}`} />
                  <Stat label="TEA" value={`${(result.annualInterestRate * 100).toFixed(0)}%`} />
                </div>
              );
            })()}

            <div className="mt-4">
              <button
                onClick={handleApply}
                disabled={applying || applied}
                className="bg-green-600 hover:bg-green-700 disabled:opacity-50 text-white font-semibold px-6 py-2 rounded-lg text-sm transition-colors"
              >
                {applied ? "✓ Solicitud enviada" : applying ? "Enviando..." : "Solicitar préstamo"}
              </button>
            </div>
          </div>

          <PaymentScheduleTable schedule={result.schedule} />
        </div>
      )}
    </div>
  );
}

function inputClass(hasError: boolean) {
  return `border rounded-lg px-3 py-2 text-sm w-full focus:outline-none focus:ring-2 focus:ring-blue-400 ${
    hasError ? "border-red-400 bg-red-50" : "border-gray-300"
  }`;
}

function Field({
  label,
  hint,
  error,
  children,
}: {
  label: string;
  hint?: string;
  error?: string;
  children: React.ReactNode;
}) {
  return (
    <div className="flex flex-col gap-1">
      <span className="text-sm text-gray-600">{label}</span>
      {children}
      {error ? (
        <span className="text-xs text-red-500">{error}</span>
      ) : hint ? (
        <span className="text-xs text-gray-400">{hint}</span>
      ) : null}
    </div>
  );
}

function Stat({ label, value, highlight }: { label: string; value: string; highlight?: boolean }) {
  return (
    <div className="flex flex-col gap-0.5">
      <span className="text-xs text-gray-500">{label}</span>
      <span className={`font-semibold text-lg ${highlight ? "text-blue-600" : "text-gray-800"}`}>
        {value}
      </span>
    </div>
  );
}
