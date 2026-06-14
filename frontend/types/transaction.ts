export type TransactionType = 'Disbursement' | 'Payment' | 'Transfer';

export type TransactionStatus = 'Pending' | 'Completed' | 'Failed';

export interface TransactionResponse {
  id: string;
  idempotencyKey: string;
  type: TransactionType;
  amount: number;
  status: TransactionStatus;
  loanId: string | null;
  description: string;
  createdAt: string;
}

export interface CreateTransactionRequest {
  idempotencyKey: string;
  type: TransactionType;
  amount: number;
  loanId?: string;
  description: string;
}

export interface GetTransactionsParams {
  type?: TransactionType;
  status?: TransactionStatus;
}
