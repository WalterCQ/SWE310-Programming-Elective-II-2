import { ScanText, Sparkles } from 'lucide-react'
import { formatCurrency } from '../utils/formatters'
import { ErrorBanner } from './ErrorBanner'
import { PixelButton } from './PixelButton'

export function AiReceiptAssistPanel({ analyzing, error, file, onAnalyze, onApply, result }) {
  return (
    <div className="rounded-[4px_11px_5px_13px/10px_4px_12px_5px] border-2 border-stamp-green/70 bg-paper-card/95 p-4 text-ink shadow-[2px_3px_0_rgba(74,156,106,0.22),4px_8px_18px_rgba(27,37,64,0.10)]">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <p className="text-base leading-5 tracking-[0.18em] text-stamp-green">LLM vision</p>
          <h2 className="mt-1 font-title text-4xl font-bold leading-none text-ink">Auto Fill Receipt</h2>
        </div>
        <PixelButton disabled={analyzing || !file} icon={ScanText} onClick={onAnalyze} variant="amber">
          {analyzing ? 'Reading' : 'Analyze'}
        </PixelButton>
      </div>

      {analyzing ? (
        <div className="mt-4 rounded-[3px_9px_4px_10px/8px_3px_10px_4px] border-2 border-ink/35 bg-paper-soft/70 p-3">
          <div className="grid grid-cols-10 gap-1 border-2 border-stamp-green/70 bg-paper-card p-1" aria-hidden="true">
            {Array.from({ length: 10 }).map((_, index) => (
              <span className={`h-3 ${index % 2 === 0 ? 'animate-pulse bg-stamp-green' : 'bg-ink/10'}`} key={index} />
            ))}
          </div>
          <p className="mt-3 text-base leading-5 text-stamp-green">LLM is reading the image...</p>
        </div>
      ) : null}

      <div className="mt-4">
        <ErrorBanner error={error} />
      </div>

      {result ? (
        <div className="mt-4 space-y-4 text-base leading-6 text-ink-soft">
          <div className="grid gap-3 sm:grid-cols-2">
            <AiValue label="Receipt No." value={result.receiptNumber || 'Not found'} />
            <AiValue label="Date" value={result.receiptDate || 'Not found'} />
            <AiValue label="Vendor" value={result.vendorName || 'Not found'} />
            <AiValue label="Tax" value={formatCurrency(result.taxAmount ?? 0)} />
            <AiValue label="Total" value={formatCurrency(result.totalAmount ?? 0)} />
            <AiValue label="Confidence" value={`${Math.round((result.confidence ?? 0) * 100)}%`} />
          </div>
          <div className="max-h-44 overflow-auto rounded-[3px_8px_4px_9px/7px_3px_9px_4px] border-2 border-dashed border-ink/30 bg-paper-soft/80 p-3 text-sm leading-5 text-ink-muted">
            {result.rawTextSummary || 'No readable summary returned.'}
          </div>
          <PixelButton icon={Sparkles} onClick={onApply}>
            Apply Again
          </PixelButton>
        </div>
      ) : (
        <p className="mt-4 text-base leading-6 text-ink-muted">
          Upload a receipt photo and the LLM vision model will fill the form. Review everything before saving.
        </p>
      )}
    </div>
  )
}

function AiValue({ label, value }) {
  return (
    <div className="rounded-[3px_8px_4px_9px/7px_3px_9px_4px] border-2 border-ink/30 bg-paper-soft/80 p-3">
      <p className="text-sm leading-5 text-ink-muted">{label}</p>
      <p className="mt-2 break-words text-base leading-5 text-ink">{value}</p>
    </div>
  )
}
