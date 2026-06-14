import Link from "next/link";

export default function Home() {
  return (
    <div className="flex flex-col items-center justify-center py-20 gap-8 text-center">
      <div>
        <h1 className="text-4xl font-bold text-gray-900">SGIP</h1>
        <p className="mt-2 text-gray-500 text-lg">
          Sistema de Gestion de Inversiones y Préstamos
        </p>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 w-full max-w-lg">
        <Link
          href="/loans/simulate"
          className="flex flex-col gap-1 p-6 bg-white rounded-xl border border-gray-200 hover:border-blue-400 hover:shadow-sm transition-all"
        >
          <span className="text-2xl">🧮</span>
          <span className="font-semibold text-gray-800">Simulador</span>
          <span className="text-sm text-gray-500">
            Calcula tu cuota mensual y cronograma de pagos
          </span>
        </Link>

        <Link
          href="/loans"
          className="flex flex-col gap-1 p-6 bg-white rounded-xl border border-gray-200 hover:border-blue-400 hover:shadow-sm transition-all"
        >
          <span className="text-2xl">📋</span>
          <span className="font-semibold text-gray-800">Prestamos</span>
          <span className="text-sm text-gray-500">
            Revisa el estado de las solicitudes
          </span>
        </Link>

        <Link
          href="/transactions"
          className="flex flex-col gap-1 p-6 bg-white rounded-xl border border-gray-200 hover:border-blue-400 hover:shadow-sm transition-all sm:col-span-2"
        >
          <span className="text-2xl">💳</span>
          <span className="font-semibold text-gray-800">Transacciones</span>
          <span className="text-sm text-gray-500">
            Historial de desembolsos y pagos
          </span>
        </Link>
      </div>
    </div>
  );
}
