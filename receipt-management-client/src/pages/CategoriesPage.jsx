import { FolderOpen, Pencil, Plus, Trash2 } from 'lucide-react'
import { useCallback, useMemo, useState } from 'react'
import { getApiError } from '../api/httpClient'
import { receiptManagementApi } from '../api/receiptManagementApi'
import { CategoryIcon } from '../components/CategoryIcon'
import { ConfirmDialog } from '../components/ConfirmDialog'
import { EmptyState } from '../components/EmptyState'
import { ErrorBanner } from '../components/ErrorBanner'
import { LoadingState } from '../components/LoadingState'
import { PageHeader } from '../components/PageHeader'
import { Panel } from '../components/Panel'
import { PixelButton } from '../components/PixelButton'
import { useLoadData } from '../hooks/useLoadData'
import { formatCurrency } from '../utils/formatters'

export function CategoriesPage() {
  const loadData = useCallback(
    () => Promise.all([receiptManagementApi.getCategories(), receiptManagementApi.getReceipts()]),
    [],
  )
  const { data, error, loading, reload, setError } = useLoadData(loadData, [[], []])
  const [categories, receipts] = data
  const [deleteTarget, setDeleteTarget] = useState(null)
  const [deleting, setDeleting] = useState(false)

  const deleteCategory = () => {
    setDeleting(true)
    receiptManagementApi
      .deleteCategory(deleteTarget.expenseCategoryId)
      .then(() => {
        setDeleteTarget(null)
        reload()
      })
      .catch((apiError) => setError(getApiError(apiError)))
      .finally(() => setDeleting(false))
  }

  const currentMonthSpendByCategory = useMemo(() => buildCurrentMonthSpendByCategory(receipts), [receipts])

  if (loading) {
    return <LoadingState label="Loading categories..." />
  }

  return (
    <div className="space-y-6">
      <PageHeader
        actions={<PixelButton icon={Plus} to="/categories/new">New Category</PixelButton>}
        eyebrow="Budget Lanes"
        title="Expense Categories"
      />
      <ErrorBanner error={error} />
      {categories.length === 0 ? (
        <EmptyState
          action={<PixelButton icon={Plus} to="/categories/new">New Category</PixelButton>}
          icon={FolderOpen}
          message="Create categories so receipt spending can be grouped."
          title="No categories yet"
        />
      ) : (
        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
          {categories.map((category) => (
            <Panel className="space-y-5" key={category.expenseCategoryId}>
              <div className="flex items-start justify-between gap-4">
                <div className="flex min-w-0 items-start gap-3">
                  <CategoryIcon color={category.colorHex} iconName={category.iconName} label={`${category.name} icon`} />
                  <div className="min-w-0">
                    <h2 className="pt-1 break-words font-title text-4xl font-bold leading-none text-ink">{category.name}</h2>
                  </div>
                </div>
                <svg className="h-9 w-9 rounded-[3px_8px_4px_9px/7px_3px_9px_4px] border-2 border-ink/45" role="img" aria-label={`${category.name} color swatch`}>
                  <rect fill={category.colorHex} height="100%" width="100%" />
                </svg>
              </div>
              <p className="min-h-12 text-lg leading-7 text-ink-soft">{category.description ?? 'No description provided.'}</p>
              <div className="space-y-3">
                <p className="text-sm leading-5 text-ink-muted">Monthly Budget</p>
                <p className="mt-2 font-title text-4xl font-bold leading-none text-ink">{formatCurrency(category.monthlyBudget)}</p>
                <BudgetProgress category={category} usedAmount={currentMonthSpendByCategory[category.expenseCategoryId] ?? 0} />
              </div>
              <div className="flex flex-wrap gap-2">
                <PixelButton icon={Pencil} to={`/categories/${category.expenseCategoryId}/edit`} variant="ghost">Edit</PixelButton>
                <PixelButton icon={Trash2} onClick={() => setDeleteTarget(category)} variant="danger">Delete</PixelButton>
              </div>
            </Panel>
          ))}
        </div>
      )}
      <ConfirmDialog
        confirming={deleting}
        message="Deleting this category keeps old receipts through their stored category snapshot."
        onCancel={() => setDeleteTarget(null)}
        onConfirm={deleteCategory}
        open={Boolean(deleteTarget)}
        title={`Delete ${deleteTarget?.name ?? 'category'}?`}
      />
    </div>
  )
}

function BudgetProgress({ category, usedAmount }) {
  const budget = Number(category.monthlyBudget ?? 0)
  const progress = budget > 0 ? Math.min((usedAmount / budget) * 100, 100) : 0
  const isOverBudget = budget > 0 && usedAmount > budget
  const label = budget > 0 ? `${Math.round((usedAmount / budget) * 100)}% used` : 'No budget set'

  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between gap-3 text-sm leading-5 text-ink-muted">
        <span>{formatCurrency(usedAmount)} used this month</span>
        <span className={isOverBudget ? 'text-pencil-red' : 'text-ink-muted'}>{label}</span>
      </div>
      <div
        aria-label={`${category.name} monthly budget usage`}
        aria-valuemax={budget}
        aria-valuemin={0}
        aria-valuenow={Math.min(usedAmount, budget)}
        className="h-3 overflow-hidden rounded-[2px_6px_3px_7px/6px_2px_7px_3px] border-2 border-ink/30 bg-paper-soft"
        role="progressbar"
      >
        <div
          className="h-full transition-[width] duration-300"
          style={{
            backgroundColor: isOverBudget ? '#D6394A' : category.colorHex,
            width: `${progress}%`,
          }}
        />
      </div>
    </div>
  )
}

function buildCurrentMonthSpendByCategory(receipts) {
  const now = new Date()

  return receipts.reduce((groups, receipt) => {
    const receiptDate = new Date(receipt.receiptDate)
    if (receiptDate.getFullYear() !== now.getFullYear() || receiptDate.getMonth() !== now.getMonth()) {
      return groups
    }

    const categoryId = receipt.expenseCategoryId
    if (!categoryId) {
      return groups
    }

    groups[categoryId] ??= 0
    groups[categoryId] += Number(receipt.totalAmount ?? 0)
    return groups
  }, {})
}
