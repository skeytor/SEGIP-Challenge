"use client";

import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { TransactionStatus, TransactionType } from "@/types/transaction";

const TYPES: TransactionType[] = ["Disbursement", "Payment", "Transfer"];
const STATUSES: TransactionStatus[] = ["Pending", "Completed", "Failed"];

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

const filtersSchema = z.object({
  type: z.enum(["", "Disbursement", "Payment", "Transfer"]),
  status: z.enum(["", "Pending", "Completed", "Failed"]),
});

type FiltersFields = z.infer<typeof filtersSchema>;

interface Props {
  defaultType?: TransactionType;
  defaultStatus?: TransactionStatus;
}

export default function TransactionFilters({ defaultType, defaultStatus }: Props) {
  const router = useRouter();

  const { register, handleSubmit } = useForm<FiltersFields>({
    resolver: zodResolver(filtersSchema),
    defaultValues: {
      type: defaultType ?? "",
      status: defaultStatus ?? "",
    },
  });

  function onFilter(data: FiltersFields) {
    const params = new URLSearchParams();
    if (data.type) params.set("type", data.type);
    if (data.status) params.set("status", data.status);
    const qs = params.size > 0 ? `?${params.toString()}` : "";
    router.push(`/transactions${qs}`);
  }

  return (
    <form
      onSubmit={handleSubmit(onFilter)}
      className="flex flex-wrap gap-3 mb-6"
    >
      <select
        {...register("type")}
        className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
      >
        <option value="">Todos los tipos</option>
        {TYPES.map((t) => (
          <option key={t} value={t}>{typeLabel[t]}</option>
        ))}
      </select>

      <select
        {...register("status")}
        className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
      >
        <option value="">Todos los estados</option>
        {STATUSES.map((s) => (
          <option key={s} value={s}>{statusLabel[s]}</option>
        ))}
      </select>

      <button
        type="submit"
        className="bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold px-4 py-2 rounded-lg transition-colors"
      >
        Filtrar
      </button>
    </form>
  );
}
