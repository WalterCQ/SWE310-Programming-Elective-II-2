import { useEffect, useMemo, useRef, useState } from 'react'
import { Area, AreaChart, Bar, BarChart, CartesianGrid, Cell, Pie, PieChart, Tooltip, XAxis, YAxis } from 'recharts'
import { receiptManagementApi } from '../api/receiptManagementApi'
import { ErrorBanner } from '../components/ErrorBanner'
import { LoadingState } from '../components/LoadingState'
import { PageHeader } from '../components/PageHeader'
import { Panel } from '../components/Panel'
import { ReceiptTreemap } from '../components/ReceiptTreemap'
import { StatCard } from '../components/StatCard'
import { ledgerColors } from '../utils/constants'
import { formatCurrency } from '../utils/formatters'
import { getStatusStyle } from '../utils/statusStyles'

export function DashboardPage() {
  const [state, setState] = useState({ loading: true, error: null, receipts: [], vendors: [], categories: [] })

  useEffect(() => {
    Promise.all([receiptManagementApi.getReceipts(), receiptManagementApi.getVendors(), receiptManagementApi.getCategories()])
      .then(([receipts, vendors, categories]) => setState({ loading: false, error: null, receipts, vendors, categories }))
      .catch((error) => setState((current) => ({ ...current, loading: false, error: { message: error.message } })))
  }, [])

  const metrics = useMemo(() => buildDashboardMetrics(state.receipts), [state.receipts])

  if (state.loading) {
    return <LoadingState label="Opening the ledger..." />
  }

  return (
    <div className="space-y-6">
      <PageHeader
        eyebrow="Personal Receipt Desk"
        title="This Month's Paper Trail"
      />

      <ErrorBanner error={state.error} />

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard label="Total Spend" value={formatCurrency(metrics.totalSpend)} tone="cyan" />
        <StatCard label="Receipt Count" value={state.receipts.length} tone="red" />
        <StatCard label="Average Receipt" value={formatCurrency(metrics.averageSpend)} tone="amber" />
        <StatCard label="Active Vendors" value={state.vendors.length} tone="green" />
      </div>

      <div className="grid gap-6 xl:grid-cols-[1.1fr_0.9fr]">
        <Panel className="flex h-full flex-col">
          <ChartTitle eyebrow="Monthly pulse" title="Spend Trend" />
          <MeasuredChart className="mt-5 min-h-[300px] flex-1">
            {({ height, width }) => (
              <AreaChart data={metrics.monthlyTrend} height={height} margin={{ left: 6, right: 12, top: 10, bottom: 0 }} width={width}>
                <CartesianGrid stroke="#1B2540" strokeDasharray="5 6" strokeOpacity={0.12} vertical={false} />
                <XAxis dataKey="month" stroke="#4A5374" tick={{ fontSize: 13, fill: '#4A5374' }} tickLine={false} />
                <YAxis stroke="#4A5374" tick={{ fontSize: 13, fill: '#4A5374' }} tickLine={false} />
                <Tooltip content={<ChartTooltip />} />
                <Area dataKey="total" fill="#4A9C6A" fillOpacity={0.18} stroke="#1B2540" strokeWidth={2.5} type="monotone" />
              </AreaChart>
            )}
          </MeasuredChart>
        </Panel>

        <Panel>
          <ChartTitle eyebrow="Where it went" title="Spend by Category" />
          <MeasuredChart className="mt-5 h-[260px]">
            {({ height, width }) => (
              <BarChart data={metrics.categorySpend} height={height} margin={{ left: 6, right: 12, top: 10, bottom: 0 }} width={width}>
                <CartesianGrid stroke="#1B2540" strokeDasharray="5 6" strokeOpacity={0.12} vertical={false} />
                <XAxis dataKey="name" hide />
                <YAxis stroke="#4A5374" tick={{ fontSize: 13, fill: '#4A5374' }} tickLine={false} />
                <Tooltip content={<ChartTooltip />} />
                <Bar dataKey="total">
                  {metrics.categorySpend.map((entry, index) => (
                    <Cell fill={ledgerColors[index % ledgerColors.length]} key={entry.name} />
                  ))}
                </Bar>
              </BarChart>
            )}
          </MeasuredChart>
          <CategoryLegend data={metrics.categorySpend} />
        </Panel>
      </div>

      <div className="grid gap-6 xl:grid-cols-[0.9fr_1.1fr]">
        <Panel>
          <ChartTitle eyebrow="Status split" title="Receipt States" />
          <MeasuredChart className="mt-5 h-[230px]">
            {({ height, width }) => (
              <PieChart height={height} width={width}>
                <Pie cx="50%" cy="50%" data={metrics.statusDistribution} dataKey="value" innerRadius={55} outerRadius={95} paddingAngle={4}>
                  {metrics.statusDistribution.map((entry, index) => (
                    <Cell fill={getStatusStyle(entry.name).fill} key={entry.name ?? index} />
                  ))}
                </Pie>
                <Tooltip content={<ChartTooltip />} />
              </PieChart>
            )}
          </MeasuredChart>
          <StatusLegend data={metrics.statusDistribution} />
        </Panel>

        <ReceiptTreemap receipts={state.receipts} />
      </div>
    </div>
  )
}

function ChartTitle({ eyebrow, title }) {
  return (
    <div>
      <p className="text-base leading-5 tracking-[0.18em] text-tape-gold">{eyebrow}</p>
      <h2 className="mt-2 font-title text-4xl font-bold leading-none text-ink">{title}</h2>
    </div>
  )
}

function MeasuredChart({ children, className }) {
  const containerRef = useRef(null)
  const [size, setSize] = useState({ height: 0, width: 0 })

  useEffect(() => {
    const container = containerRef.current
    if (!container) return undefined

    const updateSize = () => {
      const rect = container.getBoundingClientRect()
      const nextSize = {
        height: Math.round(rect.height),
        width: Math.round(rect.width),
      }

      if (nextSize.height > 0 && nextSize.width > 0) {
        setSize((current) => (
          current.height === nextSize.height && current.width === nextSize.width ? current : nextSize
        ))
      }
    }

    updateSize()

    if (typeof ResizeObserver === 'undefined') {
      window.addEventListener('resize', updateSize)
      return () => window.removeEventListener('resize', updateSize)
    }

    const observer = new ResizeObserver(updateSize)
    observer.observe(container)
    return () => observer.disconnect()
  }, [])

  return (
    <div className={`${className} min-w-0`} ref={containerRef}>
      {size.height && size.width ? (
        children(size)
      ) : (
        <div className="h-full rounded-[3px_8px_4px_9px/7px_3px_9px_4px] border-2 border-dashed border-ink/30 bg-paper-soft/70" />
      )}
    </div>
  )
}

function CategoryLegend({ data }) {
  if (!data.length) {
    return <p className="mt-4 rounded-[3px_8px_4px_9px/7px_3px_9px_4px] border-2 border-dashed border-ink/30 bg-paper-soft/70 p-3 text-ink-faint">No category data</p>
  }

  return (
    <div className="mt-4 grid gap-2 sm:grid-cols-2">
      {data.slice(0, 6).map((entry, index) => (
        <div className="flex min-w-0 items-center gap-2 rounded-[3px_8px_4px_9px/7px_3px_9px_4px] border-2 border-dashed border-ink/25 bg-paper-soft/70 px-3 py-2 text-base leading-5" key={entry.name}>
          <span aria-hidden="true" className="h-3 w-3 shrink-0 rounded-full border border-ink/20" style={{ backgroundColor: ledgerColors[index % ledgerColors.length] }} />
          <span className="min-w-0 flex-1 truncate text-ink-soft">{entry.name}</span>
          <span className="shrink-0 text-ink">{formatCurrency(entry.total)}</span>
        </div>
      ))}
    </div>
  )
}

function StatusLegend({ data }) {
  if (!data.length) {
    return <p className="mt-4 rounded-[3px_8px_4px_9px/7px_3px_9px_4px] border-2 border-dashed border-ink/30 bg-paper-soft/70 p-3 text-ink-faint">No status data</p>
  }

  const total = data.reduce((sum, entry) => sum + entry.value, 0)

  return (
    <div className="mt-4 grid gap-2 sm:grid-cols-2">
      {data.map((entry) => {
        const statusStyle = getStatusStyle(entry.name)
        const percentage = total ? Math.round((entry.value / total) * 100) : 0

        return (
          <div className={`flex items-center justify-between gap-3 rounded-[3px_8px_4px_9px/7px_3px_9px_4px] border-2 px-3 py-2 text-base leading-5 ${statusStyle.badge}`} key={entry.name}>
            <span>{entry.name}</span>
            <span>{entry.value} ({percentage}%)</span>
          </div>
        )
      })}
    </div>
  )
}

function ChartTooltip({ active, payload, label }) {
  if (!active || !payload?.length) {
    return null
  }

  return (
    <div className="rounded-[3px_9px_4px_10px/8px_3px_10px_4px] border-2 border-ink bg-paper-card p-3 font-hand text-base leading-5 text-ink shadow-[2px_3px_0_rgba(27,37,64,0.18)]">
      <p className="text-ink-muted">{label ?? payload[0].name}</p>
      <p>{payload[0].dataKey === 'value' ? payload[0].value : formatCurrency(payload[0].value)}</p>
    </div>
  )
}

function buildDashboardMetrics(receipts) {
  const totalSpend = receipts.reduce((sum, receipt) => sum + Number(receipt.totalAmount ?? 0), 0)
  const averageSpend = receipts.length ? totalSpend / receipts.length : 0

  const categorySpend = Object.values(
    receipts.reduce((groups, receipt) => {
      const name = receipt.categoryName ?? 'Uncategorized'
      groups[name] ??= { name, total: 0 }
      groups[name].total += Number(receipt.totalAmount ?? 0)
      return groups
    }, {}),
  ).sort((a, b) => b.total - a.total)

  const statusDistribution = Object.values(
    receipts.reduce((groups, receipt) => {
      const name = receipt.status ?? 'Unknown'
      groups[name] ??= { name, value: 0 }
      groups[name].value += 1
      return groups
    }, {}),
  )

  const monthlyTrend = Object.values(
    receipts.reduce((groups, receipt) => {
      const date = new Date(receipt.receiptDate)
      const month = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`
      groups[month] ??= { month, total: 0 }
      groups[month].total += Number(receipt.totalAmount ?? 0)
      return groups
    }, {}),
  ).sort((a, b) => a.month.localeCompare(b.month))

  return { totalSpend, averageSpend, categorySpend, statusDistribution, monthlyTrend }
}
