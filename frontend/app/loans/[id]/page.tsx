import { loanService } from "@/services/loan-service";
import PaymentScheduleTable from "@/components/PaymentScheduleTable";
import { notFound } from "next/navigation";

export const dynamic = "force-dynamic";

const statusLabel: Record<string, string> = {
  Pending: "Pendiente",
  Approved: "Aprobado",
  Rejected: "Rechazado",
  Active: "Activo",
};

const statusClass: Record<string, string> = {
  Pending: "bg-yellow-100 text-yellow-700",
  Approved: "bg-blue-100 text-blue-700",
  Rejected: "bg-red-100 text-red-700",
  Active: "bg-green-100 text-green-700",
};

interface Props {
  params: { id: string };
}

export default async function LoanDetailPage({ params }: Props) {
  const [loan, schedule] = await Promise.all([
    loanService.getById(params.id).catch(() => null),
    loanService.getSchedule(params.id).catch(() => []),
  ]);

  if (!loan) notFound();

  return (
    <div className="space-y-6">
      <div className="bg-white rounded-xl border border-gray-200 p-6">
        <div className="flex items-start justify-between mb-4">
          <h1 className="text-2xl font-bold text-gray-900">Préstamo</h1>
          <span className={`px-3 py-1 rounded-full text-sm font-medium ${statusClass[loan.status]}`}>
            {statusLabel[loan.status]}
          </span>
        </div>

        <div className="grid grid-cols-2 sm:grid-cols-4 gap-6">
          <div>
            <p className="text-xs text-gray-500">Monto</p>
            <p className="font-semibold text-gray-800">${loan.amount.toLocaleString("es")}</p>
          </div>
          <div>
            <p className="text-xs text-gray-500">Plazo</p>
            <p className="font-semibold text-gray-800">{loan.termMonths} meses</p>
          </div>
          <div>
            <p className="text-xs text-gray-500">Cuota mensual</p>
            <p className="font-semibold text-gray-800">${loan.monthlyPayment.toFixed(2)}</p>
          </div>
          <div>
            <p className="text-xs text-gray-500">TEA</p>
            <p className="font-semibold text-gray-800">{(loan.annualInterestRate * 100).toFixed(0)}%</p>
          </div>
        </div>
      </div>

      {schedule.length > 0 && <PaymentScheduleTable schedule={schedule} />}
    </div>
  );
}
