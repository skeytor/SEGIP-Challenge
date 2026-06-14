import { transactionService } from "@/services/transaction-service";
import TransactionList from "@/components/TransactionList";
import TransactionFilters from "@/components/TransactionFilters";
import { TransactionStatus, TransactionType } from "@/types/transaction";

export const dynamic = "force-dynamic";

const TYPES: TransactionType[] = ["Disbursement", "Payment", "Transfer"];
const STATUSES: TransactionStatus[] = ["Pending", "Completed", "Failed"];

interface Props {
  searchParams: { type?: string; status?: string };
}

export default async function TransactionsPage({ searchParams }: Props) {
  const type = TYPES.includes(searchParams.type as TransactionType)
    ? (searchParams.type as TransactionType)
    : undefined;
  const status = STATUSES.includes(searchParams.status as TransactionStatus)
    ? (searchParams.status as TransactionStatus)
    : undefined;

  const transactions = await transactionService.getAll({ type, status }).catch(() => []);

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Transacciones</h1>
      <TransactionFilters defaultType={type} defaultStatus={status} />
      <TransactionList transactions={transactions} />
    </div>
  );
}
