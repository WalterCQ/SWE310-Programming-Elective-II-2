import { currencyCode } from './constants'

export function formatCurrency(value) {
  return new Intl.NumberFormat('en-MY', {
    style: 'currency',
    currency: currencyCode,
  }).format(Number(value ?? 0))
}

export function formatDate(value) {
  if (!value) {
    return '-'
  }

  return new Intl.DateTimeFormat('en-MY', {
    year: 'numeric',
    month: 'short',
    day: '2-digit',
  }).format(new Date(value))
}

export function toDateInputValue(value) {
  if (!value) {
    return new Date().toISOString().slice(0, 10)
  }

  return new Date(value).toISOString().slice(0, 10)
}

export function parseMoney(value) {
  const parsed = Number(value)
  return Number.isFinite(parsed) ? Math.round(parsed * 100) / 100 : 0
}

export function calculateLineTotal(item) {
  return parseMoney(parseMoney(item.quantity) * parseMoney(item.unitPrice))
}

export function calculateReceiptTotals(items, taxAmount) {
  const subtotalAmount = items.reduce((sum, item) => sum + calculateLineTotal(item), 0)
  const roundedSubtotal = parseMoney(subtotalAmount)
  const roundedTax = parseMoney(taxAmount)

  return {
    subtotalAmount: roundedSubtotal,
    taxAmount: roundedTax,
    totalAmount: parseMoney(roundedSubtotal + roundedTax),
  }
}
