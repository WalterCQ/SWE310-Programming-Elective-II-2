import { useEffect, useRef, useState } from 'react'
import { Treemap, Tooltip } from 'recharts'
import { ledgerColors } from '../utils/constants'
import { formatCurrency, formatDate } from '../utils/formatters'

export function ReceiptTreemap({ receipts }) {
  const chartRef = useRef(null)
  const [chartSize, setChartSize] = useState({ height: 256, width: 0 })
  const data = receipts
    .slice()
    .sort((a, b) => new Date(b.receiptDate) - new Date(a.receiptDate))
    .slice(0, 18)
    .map((receipt, index) => ({
      name: receipt.vendorName ?? receipt.receiptNumber ?? `Receipt ${index + 1}`,
      value: Math.max(Number(receipt.totalAmount ?? 0), 0.01),
      receiptNumber: receipt.receiptNumber,
      receiptDate: receipt.receiptDate,
      categoryName: receipt.categoryName ?? 'Uncategorized',
      fill: ledgerColors[index % ledgerColors.length],
    }))

  useEffect(() => {
    const chart = chartRef.current
    if (!chart) return undefined

    const updateSize = () => {
      const rect = chart.getBoundingClientRect()
      if (rect.width > 0 && rect.height > 0) {
        setChartSize({ height: Math.round(rect.height), width: Math.round(rect.width) })
      }
    }

    updateSize()

    if (typeof ResizeObserver === 'undefined') {
      window.addEventListener('resize', updateSize)
      return () => window.removeEventListener('resize', updateSize)
    }

    const observer = new ResizeObserver(updateSize)
    observer.observe(chart)
    return () => observer.disconnect()
  }, [])

  return (
    <div className="flex h-full flex-col rounded-[4px_11px_5px_13px/10px_4px_12px_5px] border-2 border-ink/70 bg-paper-card/95 p-4 text-ink shadow-[2px_3px_0_rgba(27,37,64,0.18),4px_8px_20px_rgba(27,37,64,0.10)]">
      <div className="mb-4 shrink-0">
        <p className="text-base leading-5 tracking-[0.18em] text-tape-gold">Latest receipts</p>
        <h2 className="font-title text-4xl font-bold leading-none text-ink">Receipt Treemap</h2>
      </div>

      <div className="min-h-64 min-w-0 flex-1" ref={chartRef}>
        {data.length && chartSize.width ? (
          <Treemap
            content={<ReceiptTreemapTile />}
            data={data}
            dataKey="value"
            height={chartSize.height}
            isAnimationActive
            nameKey="name"
            stroke="#fffdf7"
            width={chartSize.width}
          >
            <Tooltip content={<ReceiptTreemapTooltip />} />
          </Treemap>
        ) : data.length ? (
          <div className="h-full rounded-[3px_8px_4px_9px/7px_3px_9px_4px] border-2 border-dashed border-ink/30 bg-paper-soft/70" />
        ) : (
          <div className="grid h-full place-items-center rounded-[3px_8px_4px_9px/7px_3px_9px_4px] border-2 border-dashed border-ink/30 bg-paper-soft/70 text-ink-faint">
            No receipt data
          </div>
        )}
      </div>
    </div>
  )
}

function ReceiptTreemapTile({ x, y, width, height, name, value, fill }) {
  if (width <= 0 || height <= 0) return null

  const showName = width > 74 && height > 42
  const showValue = width > 92 && height > 70

  return (
    <g>
      <rect
        x={x + 1}
        y={y + 1}
        width={Math.max(width - 2, 0)}
        height={Math.max(height - 2, 0)}
        rx={5}
        ry={5}
        fill={fill}
        opacity={0.9}
        stroke="#fffdf7"
        strokeWidth={2}
      />
      {showName ? (
        <text x={x + 10} y={y + 22} fill="#fffdf7" fontFamily="Patrick Hand, Comic Sans MS, cursive" fontSize={16}>
          {truncateLabel(name, Math.max(8, Math.floor(width / 8)))}
        </text>
      ) : null}
      {showValue ? (
        <text x={x + 10} y={y + 44} fill="#fffdf7" fontFamily="Patrick Hand, Comic Sans MS, cursive" fontSize={14} opacity={0.88}>
          {formatCurrency(value)}
        </text>
      ) : null}
    </g>
  )
}

function ReceiptTreemapTooltip({ active, payload }) {
  if (!active || !payload?.length) {
    return null
  }

  const receipt = payload[0].payload

  return (
    <div className="rounded-[3px_9px_4px_10px/8px_3px_10px_4px] border-2 border-ink bg-paper-card p-3 font-hand text-base leading-5 text-ink shadow-[2px_3px_0_rgba(27,37,64,0.18)]">
      <p className="text-ink-muted">{receipt.name}</p>
      <p>{formatCurrency(receipt.value)}</p>
      <p className="mt-1 text-sm leading-5 text-ink-faint">{receipt.categoryName}</p>
      <p className="text-sm leading-5 text-ink-faint">{formatDate(receipt.receiptDate)}</p>
    </div>
  )
}

function truncateLabel(value, maxLength) {
  const label = String(value ?? '')
  return label.length > maxLength ? `${label.slice(0, Math.max(maxLength - 1, 1))}...` : label
}
