export type LoanStatus = 'Pending' | 'Approved' | 'Rejected' | 'Active';

export type LoanType = 'Fixed' | 'Decreasing';

export type PaymentScheduleStatus = 'Pending' | 'Paid';

export interface LoanResponse {
  id: string;
  userId: string;
  amount: number;
  termMonths: number;
  annualInterestRate: number;
  loanType: LoanType;
  status: LoanStatus;
  monthlyPayment: number;
  createdAt: string;
  updatedAt: string;
}

export interface PaymentInstallment {
  paymentNumber: number;
  dueDate: string;
  totalPayment: number;
  principal: number;
  interest: number;
  remainingBalance: number;
}

export interface SimulateLoanResponse {
  amount: number;
  termMonths: number;
  annualInterestRate: number;
  monthlyPayment: number;
  loanType: LoanType;
  schedule: PaymentInstallment[];
}

export interface PaymentScheduleResponse {
  paymentNumber: number;
  dueDate: string;
  totalPayment: number;
  principal: number;
  interest: number;
  remainingBalance: number;
  status: PaymentScheduleStatus;
}

export interface SimulateLoanRequest {
  amount: number;
  termMonths: number;
  loanType?: LoanType;
}

export interface CreateLoanRequest {
  amount: number;
  userId?: string;
  termMonths: number;
  loanType?: LoanType;
  monthlyIncome?: number;
}
