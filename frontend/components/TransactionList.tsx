import {
  TransactionResponse,
  TransactionStatus,
  TransactionType,
} from "@/types/transaction";

interface Props {
  transactions: TransactionResponse[];
}

const typeLabel: Record<TransactionType, string> = {
  Disbursement: "Desembolso",
  Payment: "Pago",
  Transfer: "Transferencia",
};

const statusLabel: Record<TransactionStatus, string> = {
  Pending: "Pendiente",
  Completed: "Completada",
  Failed: "Fallida",
};

const statusClass: Record<TransactionStatus, string> = {
  Pending: "bg-yellow-100 text-yellow-700",
  Completed: "bg-green-100 text-green-700",
  Failed: "bg-red-100 text-red-700",
};

export default function TransactionList({ transactions }: Props) {
  if (transactions.length === 0) {
    return (
      <div className="text-center py-16 text-gray-400">
        <p>No hay transacciones.</p>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 text-gray-500 uppercase text-xs">
            <tr>
              <th className="px-4 py-3 text-left">Tipo</th>
              <th className="px-4 py-3 text-left">Descripcion</th>
              <th className="px-4 py-3 text-right">Monto</th>
              <th className="px-4 py-3 text-center">Estado</th>
              <th className="px-4 py-3 text-right">Fecha</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {transactions.map((tx) => (
              <tr key={tx.id} className="hover:bg-gray-50">
                <td className="px-4 py-3 text-gray-700 font-medium">
                  {typeLabel[tx.type] ?? tx.type}
                </td>
                <td className="px-4 py-3 text-gray-500">{tx.description}</td>
                <td className="px-4 py-3 text-right font-semibold">
                  ${tx.amount.toFixed(2)}
                </td>
                <td className="px-4 py-3 text-center">
                  <span
                    className={`inline-block px-2 py-0.5 rounded-full text-xs font-medium ${statusClass[tx.status]}`}
                  >
                    {statusLabel[tx.status]}
                  </span>
                </td>
                <td className="px-4 py-3 text-right text-gray-400">
                  {new Date(tx.createdAt).toLocaleDateString("es")}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
