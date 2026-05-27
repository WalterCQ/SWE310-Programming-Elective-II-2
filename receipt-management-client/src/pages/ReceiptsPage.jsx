import { Eye, Pencil, Plus, ReceiptText, Trash2 } from 'lucide-react'
import { useCallback, useState } from 'react'
import { getApiError, resolveImageUrl } from '../api/httpClient'
import { receiptManagementApi } from '../api/receiptManagementApi'
import { ConfirmDialog } from '../components/ConfirmDialog'
import { EmptyState } from '../components/EmptyState'
import { ErrorBanner } from '../components/ErrorBanner'
import { LoadingState } from '../components/LoadingState'
import { PageHeader } from '../components/PageHeader'
import { Panel } from '../components/Panel'
import { PixelButton } from '../components/PixelButton'
import { StatusBadge } from '../components/StatusBadge'
import { useLoadData } from '../hooks/useLoadData'
import { formatCurrency, formatDate } from '../utils/formatters'

export function ReceiptsPage() {
  const loadReceipts = useCallback(() => receiptManagementApi.getReceipts(), [])
  const { data: receipts, error, loading, reload, setError } = useLoadData(loadReceipts, [])
  const [deleteTarget, setDeleteTarget] = useState(null)
  const [deleting, setDeleting] = useState(false)

  const deleteReceipt = () => {
    setDeleting(true)
    receiptManagementApi
      .deleteReceipt(deleteTarget.receiptId)
      .then(() => {
        setDeleteTarget(null)
        reload()
      })
      .catch((apiError) => setError(getApiError(apiError)))
      .finally(() => setDeleting(false))
  }

  if (loading) {
    return <LoadingState label="Loading receipts..." />
  }

  return (
    <div className="space-y-6">
      <PageHeader
        actions={<PixelButton icon={Plus} to="/receipts/new">New Receipt</PixelButton>}
        eyebrow="Saved Scraps"
        title="Receipts"
      />
      <ErrorBanner error={error} />

      {receipts.length === 0 ? (
        <EmptyState
          action={<PixelButton icon={Plus} to="/receipts/new">New Receipt</PixelButton>}
          icon={ReceiptText}
          message="Create a receipt after adding vendors and categories."
          title="No receipts yet"
        />
      ) : (
        <div className="grid gap-5">
          {receipts.map((receipt) => (
            <Panel className="grid gap-4 lg:grid-cols-[104px_minmax(0,1fr)_190px] lg:items-center" key={receipt.receiptId}>
              <div className="grid h-32 place-items-center overflow-hidden rounded-[3px_9px_4px_10px/8px_3px_10px_4px] border-2 border-ink/45 bg-paper-soft lg:h-24">
                {receipt.imageUrl ? (
                  <img alt={`Receipt ${receipt.receiptNumber}`} className="h-full w-full object-cover" src={resolveImageUrl(receipt.imageUrl)} />
                ) : (
                  <Eye className="text-ink-faint" size={42} />
                )}
              </div>
              <div className="min-w-0 space-y-3">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div className="min-w-0">
                    <p className="text-sm leading-5 tracking-[0.16em] text-tape-gold">{receipt.receiptNumber}</p>
                    <h2 className="mt-1 truncate font-title text-4xl font-bold leading-none text-ink lg:text-3xl">{receipt.vendorName}</h2>
                  </div>
                  <StatusBadge status={receipt.status} />
                </div>
                <div className="grid gap-2 text-base leading-5 text-ink-soft sm:grid-cols-2 lg:grid-cols-4">
                  <Info label="Date" value={formatDate(receipt.receiptDate)} />
                  <Info label="Category" value={receipt.categoryName} />
                  <Info label="Payment" value={receipt.paymentMethod} />
                  <Info label="Items" value={receipt.items?.length ?? 0} />
                </div>
              </div>
              <div className="flex flex-col justify-between gap-3 lg:items-end">
                <div className="text-left lg:text-right">
                  <p className="text-base leading-5 text-ink-muted">Total</p>
                  <p className="mt-1 font-title text-5xl font-bold leading-none text-ink lg:text-4xl">{formatCurrency(receipt.totalAmount)}</p>
                </div>
                <div className="flex flex-wrap gap-2 lg:justify-end">
                  <PixelButton icon={Pencil} to={`/receipts/${receipt.receiptId}/edit`} variant="ghost">Edit</PixelButton>
                  <PixelButton icon={Trash2} onClick={() => setDeleteTarget(receipt)} variant="danger">Delete</PixelButton>
                </div>
              </div>
            </Panel>
          ))}
        </div>
      )}

      <ConfirmDialog
        confirming={deleting}
        message="Deleting this receipt also deletes its line items."
        onCancel={() => setDeleteTarget(null)}
        onConfirm={deleteReceipt}
        open={Boolean(deleteTarget)}
        title={`Delete ${deleteTarget?.receiptNumber ?? 'receipt'}?`}
      />
    </div>
  )
}

function Info({ label, value }) {
  return (
    <div className="rounded-[3px_8px_4px_9px/7px_3px_9px_4px] border-2 border-dashed border-ink/25 bg-paper-soft/70 p-2">
      <p className="text-sm leading-5 text-ink-muted">{label}</p>
      <p className="mt-1 break-words text-ink">{value}</p>
    </div>
  )
}
