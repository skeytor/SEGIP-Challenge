import { PaymentInstallment, PaymentScheduleResponse } from "@/types/loan";

interface Props {
  schedule: PaymentInstallment[] | PaymentScheduleResponse[];
}

const statusLabel: Record<string, string> = {
  Pending: "Pendiente",
  Paid: "Pagado",
  Overdue: "Vencido",
};

const statusClass: Record<string, string> = {
  Pending: "bg-yellow-100 text-yellow-700",
  Paid: "bg-green-100 text-green-700",
  Overdue: "bg-red-100 text-red-700",
};

export default function PaymentScheduleTable({ schedule }: Props) {
  return (
    <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
      <div className="px-6 py-4 border-b border-gray-100">
        <h2 className="font-semibold text-gray-800">Cronograma de pagos</h2>
      </div>
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 text-gray-500 uppercase text-xs">
            <tr>
              <th className="px-4 py-3 text-left">#</th>
              <th className="px-4 py-3 text-right">Fecha</th>
              <th className="px-4 py-3 text-right">Cuota</th>
              <th className="px-4 py-3 text-right">Capital</th>
              <th className="px-4 py-3 text-right">Interés</th>
              <th className="px-4 py-3 text-right">Saldo</th>
              <th className="px-4 py-3 text-center">Estado</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {schedule.map((row) => {
              const status = (row as PaymentScheduleResponse).status;
              return (
                <tr key={row.paymentNumber} className="hover:bg-gray-50">
                  <td className="px-4 py-2 text-gray-500">
                    {row.paymentNumber}
                  </td>
                  <td className="px-4 py-2 text-right text-gray-600">
                    {new Date(row.dueDate).toLocaleDateString("es")}
                  </td>
                  <td className="px-4 py-2 text-right font-medium">
                    ${row.totalPayment.toFixed(2)}
                  </td>
                  <td className="px-4 py-2 text-right">
                    ${row.principal.toFixed(2)}
                  </td>
                  <td className="px-4 py-2 text-right">
                    ${row.interest.toFixed(2)}
                  </td>
                  <td className="px-4 py-2 text-right">
                    ${row.remainingBalance.toFixed(2)}
                  </td>
                  <td className="px-4 py-2 text-center">
                    {status && (
                      <span
                        className={`inline-block px-2 py-0.5 rounded-full text-xs font-medium ${statusClass[status] ?? "bg-gray-100 text-gray-600"}`}
                      >
                        {statusLabel[status] ?? status}
                      </span>
                    )}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
}
