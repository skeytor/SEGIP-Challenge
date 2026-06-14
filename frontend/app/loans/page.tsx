import { loanService } from "@/services/loan-service";
import LoanList from "@/components/LoanList";
import Link from "next/link";

export const dynamic = "force-dynamic";

export default async function LoansPage() {
  const loans = await loanService.getAll().catch(() => []);

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Ver Prestamos</h1>
        <Link
          href="/loans/simulate"
          className="bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold px-4 py-2 rounded-lg transition-colors"
        >
          + Nuevo prestamo
        </Link>
      </div>
      <LoanList loans={loans} />
    </div>
  );
}
