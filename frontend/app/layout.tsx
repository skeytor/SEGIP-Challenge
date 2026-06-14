import type { Metadata } from "next";
import Link from "next/link";
import "./globals.css";

export const metadata: Metadata = {
  title: "SGIP - Sistema de Gestión de Préstamos",
  description: "Simulación, solicitud y gestión de préstamos",
};

export default function RootLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="es">
      <body className="antialiased bg-gray-50 min-h-screen">
        <nav className="bg-white border-b border-gray-200 px-6 py-4">
          <div className="max-w-5xl mx-auto flex items-center gap-8">
            <Link href="/" className="font-bold text-lg text-blue-600">
              SGIP
            </Link>
            <Link
              href="/loans"
              className="text-sm text-gray-600 hover:text-blue-600"
            >
              Préstamos
            </Link>
            <Link
              href="/loans/simulate"
              className="text-sm text-gray-600 hover:text-blue-600"
            >
              Simulador
            </Link>
            <Link
              href="/transactions"
              className="text-sm text-gray-600 hover:text-blue-600"
            >
              Transacciones
            </Link>
          </div>
        </nav>
        <main className="max-w-5xl mx-auto px-6 py-8">{children}</main>
      </body>
    </html>
  );
}
