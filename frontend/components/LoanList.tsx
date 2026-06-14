"use client";

import Link from "next/link";
import { LoanResponse, LoanStatus } from "@/types/loan";

interface Props {
  loans: LoanResponse[];
}

const statusLabel: Record<LoanStatus, string> = {
  Pending: "Pendiente",
  Approved: "Aprobado",
  Rejected: "Rechazado",
  Active: "Activo",
};

const statusClass: Record<LoanStatus, string> = {
  Pending: "bg-yellow-100 text-yellow-700",
  Approved: "bg-blue-100 text-blue-700",
  Rejected: "bg-red-100 text-red-700",
  Active: "bg-green-100 text-green-700",
};

export default function LoanList({ loans }: Props) {
  if (loans.length === 0) {
    return (
      <div className="text-center py-16 text-gray-400">
        <p className="text-lg">No tienes préstamos aún.</p>
        <Link href="/loans/simulate" className="mt-2 inline-block text-blue-600 hover:underline text-sm">
          Simular un préstamo →
        </Link>
      </div>
    );
  }

  return (
    <div className="space-y-3">
      {loans.map((loan) => (
        <Link
          key={loan.id}
          href={`/loans/${loan.id}`}
          className="flex items-center justify-between bg-white rounded-xl border border-gray-200 px-5 py-4 hover:border-blue-300 hover:shadow-sm transition-all"
        >
          <div className="flex flex-col gap-0.5">
            <span className="font-semibold text-gray-800">${loan.amount.toLocaleString("es")}</span>
            <span className="text-sm text-gray-500">{loan.termMonths} meses · ${loan.monthlyPayment.toFixed(2)}/mes</span>
          </div>
          <div className="flex items-center gap-3">
            <span className="text-xs text-gray-400">
              {new Date(loan.createdAt).toLocaleDateString("es")}
            </span>
            <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${statusClass[loan.status]}`}>
              {statusLabel[loan.status]}
            </span>
          </div>
        </Link>
      ))}
    </div>
  );
}
