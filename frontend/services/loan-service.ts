import { api } from '@/lib/api';
import {
  CreateLoanRequest,
  LoanResponse,
  PaymentScheduleResponse,
  SimulateLoanRequest,
  SimulateLoanResponse,
} from '@/types/loan';

const BASE = '/api/loans';

export const loanService = {
  simulate: (body: SimulateLoanRequest): Promise<SimulateLoanResponse> =>
    api.post<SimulateLoanResponse>(`${BASE}/simulate`, body),

  apply: (body: CreateLoanRequest): Promise<LoanResponse> =>
    api.post<LoanResponse>(BASE, body),

  getAll: (userId?: string): Promise<LoanResponse[]> => {
    const query = userId ? `?userId=${encodeURIComponent(userId)}` : '';
    return api.get<LoanResponse[]>(`${BASE}${query}`);
  },

  getById: (id: string): Promise<LoanResponse> =>
    api.get<LoanResponse>(`${BASE}/${id}`),

  getSchedule: (id: string): Promise<PaymentScheduleResponse[]> =>
    api.get<PaymentScheduleResponse[]>(`${BASE}/${id}/schedule`),

  approve: (id: string): Promise<LoanResponse> =>
    api.patch<LoanResponse>(`${BASE}/${id}/approve`),

  reject: (id: string): Promise<LoanResponse> =>
    api.patch<LoanResponse>(`${BASE}/${id}/reject`),
};
