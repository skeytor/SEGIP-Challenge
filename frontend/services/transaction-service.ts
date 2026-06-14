import { api } from "@/lib/api";
import {
  CreateTransactionRequest,
  GetTransactionsParams,
  TransactionResponse,
} from "@/types/transaction";

const BASE = "/api/transactions";

export const transactionService = {
  create: (body: CreateTransactionRequest): Promise<TransactionResponse> =>
    api.post<TransactionResponse>(BASE, body),

  getAll: (params?: GetTransactionsParams): Promise<TransactionResponse[]> => {
    const query = new URLSearchParams();
    if (params?.type) query.set("type", params.type);
    if (params?.status) query.set("status", params.status);

    const qs = query.size > 0 ? `?${query.toString()}` : "";
    
    return api.get<TransactionResponse[]>(`${BASE}${qs}`);
  },

  getById: (id: string): Promise<TransactionResponse> =>
    api.get<TransactionResponse>(`${BASE}/${id}`),
};
