import { Pencil, Plus, Store, Trash2 } from 'lucide-react'
import { useCallback, useState } from 'react'
import { receiptManagementApi } from '../api/receiptManagementApi'
import { getApiError } from '../api/httpClient'
import { ConfirmDialog } from '../components/ConfirmDialog'
import { EmptyState } from '../components/EmptyState'
import { ErrorBanner } from '../components/ErrorBanner'
import { LoadingState } from '../components/LoadingState'
import { PageHeader } from '../components/PageHeader'
import { Panel } from '../components/Panel'
import { PixelButton } from '../components/PixelButton'
import { useLoadData } from '../hooks/useLoadData'

export function VendorsPage() {
  const loadVendors = useCallback(() => receiptManagementApi.getVendors(), [])
  const { data: vendors, error, loading, reload, setError } = useLoadData(loadVendors, [])
  const [deleteTarget, setDeleteTarget] = useState(null)
  const [deleting, setDeleting] = useState(false)

  const deleteVendor = () => {
    setDeleting(true)
    receiptManagementApi
      .deleteVendor(deleteTarget.vendorId)
      .then(() => {
        setDeleteTarget(null)
        reload()
      })
      .catch((apiError) => setError(getApiError(apiError)))
      .finally(() => setDeleting(false))
  }

  if (loading) {
    return <LoadingState label="Loading vendors..." />
  }

  return (
    <div className="space-y-6">
      <PageHeader
        actions={<PixelButton icon={Plus} to="/vendors/new">New Vendor</PixelButton>}
        eyebrow="Merchant Registry"
        title="Vendors"
      />
      <ErrorBanner error={error} />
      {vendors.length === 0 ? (
        <EmptyState
          action={<PixelButton icon={Plus} to="/vendors/new">New Vendor</PixelButton>}
          icon={Store}
          message="Add vendors before recording receipts."
          title="No vendors yet"
        />
      ) : (
        <Panel>
          <div className="overflow-x-auto">
            <table className="w-full min-w-[880px] text-left text-base leading-5">
              <thead className="border-b-2 border-dashed border-line text-ink-muted">
                <tr>
                  <th className="px-3 py-4">Name</th>
                  <th className="px-3 py-4">Contact</th>
                  <th className="px-3 py-4">Phone</th>
                  <th className="px-3 py-4">Email</th>
                  <th className="px-3 py-4">Tax No.</th>
                  <th className="px-3 py-4 text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {vendors.map((vendor) => (
                  <tr className="border-b border-dashed border-line-faint text-ink-soft hover:bg-paper-soft/80" key={vendor.vendorId}>
                    <td className="px-3 py-4 font-title text-3xl font-bold leading-none text-ink">{vendor.name}</td>
                    <td className="px-3 py-4">{vendor.contactPerson ?? '-'}</td>
                    <td className="px-3 py-4">{vendor.phone ?? '-'}</td>
                    <td className="px-3 py-4">{vendor.email ?? '-'}</td>
                    <td className="px-3 py-4">{vendor.taxRegistrationNumber ?? '-'}</td>
                    <td className="px-3 py-4">
                      <div className="flex justify-end gap-2">
                        <PixelButton icon={Pencil} to={`/vendors/${vendor.vendorId}/edit`} variant="ghost">Edit</PixelButton>
                        <PixelButton icon={Trash2} onClick={() => setDeleteTarget(vendor)} variant="danger">Delete</PixelButton>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Panel>
      )}
      <ConfirmDialog
        confirming={deleting}
        message="Deleting this vendor keeps old receipts through their stored vendor snapshot."
        onCancel={() => setDeleteTarget(null)}
        onConfirm={deleteVendor}
        open={Boolean(deleteTarget)}
        title={`Delete ${deleteTarget?.name ?? 'vendor'}?`}
      />
    </div>
  )
}
