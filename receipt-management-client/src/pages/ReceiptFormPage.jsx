import { Save, Upload } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { getApiError, resolveImageUrl } from '../api/httpClient'
import { receiptManagementApi } from '../api/receiptManagementApi'
import { ErrorBanner } from '../components/ErrorBanner'
import { FormField, inputClass, selectClass, textareaClass } from '../components/FormField'
import { LoadingState } from '../components/LoadingState'
import { AiReceiptAssistPanel } from '../components/AiReceiptAssistPanel'
import { PageHeader } from '../components/PageHeader'
import { Panel } from '../components/Panel'
import { PixelButton } from '../components/PixelButton'
import { ReceiptItemsEditor } from '../components/ReceiptItemsEditor'
import { paymentMethods, receiptImageRules, receiptStatuses, validationLimits } from '../utils/constants'
import { calculateReceiptTotals, formatCurrency, toDateInputValue } from '../utils/formatters'
import {
  validateNumberRange,
  validateOptionalText,
  validateReceiptImageFile,
  validateRequiredSelection,
  validateRequiredText,
} from '../utils/validation'

const emptyReceipt = {
  receiptNumber: '',
  receiptDate: toDateInputValue(),
  vendorId: '',
  expenseCategoryId: '',
  taxAmount: 0,
  paymentMethod: 'EWallet',
  status: 'Recorded',
  notes: '',
  imageUrl: '',
  items: [{ description: '', quantity: 1, unitPrice: 0, notes: '' }],
}

export function ReceiptFormPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const isEdit = Boolean(id)
  const [form, setForm] = useState(emptyReceipt)
  const [vendors, setVendors] = useState([])
  const [categories, setCategories] = useState([])
  const [aiFile, setAiFile] = useState(null)
  const [aiResult, setAiResult] = useState(null)
  const [aiAnalyzing, setAiAnalyzing] = useState(false)
  const [aiError, setAiError] = useState(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [uploading, setUploading] = useState(false)
  const [draggingImage, setDraggingImage] = useState(false)
  const [error, setError] = useState(null)
  const [clientErrors, setClientErrors] = useState({})

  useEffect(() => {
    const resources = [receiptManagementApi.getVendors(), receiptManagementApi.getCategories()]
    if (isEdit) {
      resources.push(receiptManagementApi.getReceipt(id))
    }

    Promise.all(resources)
      .then(([vendorData, categoryData, receipt]) => {
        setVendors(vendorData)
        setCategories(categoryData)
        if (receipt) {
          setForm({
            receiptNumber: receipt.receiptNumber ?? '',
            receiptDate: toDateInputValue(receipt.receiptDate),
            vendorId: receipt.vendorId ?? '',
            expenseCategoryId: receipt.expenseCategoryId ?? '',
            taxAmount: receipt.taxAmount ?? 0,
            paymentMethod: receipt.paymentMethod ?? 'EWallet',
            status: receipt.status ?? 'Recorded',
            notes: receipt.notes ?? '',
            imageUrl: receipt.imageUrl ?? '',
            items: receipt.items?.length
              ? receipt.items.map((item) => ({
                  description: item.description,
                  quantity: item.quantity,
                  unitPrice: item.unitPrice,
                  notes: item.notes ?? '',
                }))
              : emptyReceipt.items,
          })
        }
      })
      .catch((apiError) => setError(getApiError(apiError)))
      .finally(() => setLoading(false))
  }, [id, isEdit])

  const totals = useMemo(() => calculateReceiptTotals(form.items, form.taxAmount), [form.items, form.taxAmount])

  const updateField = (field, value) => {
    setForm((current) => ({ ...current, [field]: value }))
    setClientErrors((current) => clearFieldError(current, field))
  }

  const uploadReceiptFile = (file) => {
    if (!file) return

    const imageError = validateReceiptImageFile(file)
    if (imageError) {
      setError({ message: imageError })
      return
    }

    setAiFile(file)
    setAiResult(null)
    setAiError(null)
    setUploading(true)
    receiptManagementApi
      .uploadReceiptImage(file)
      .then((data) => updateField('imageUrl', data.imageUrl))
      .catch((apiError) => setError(getApiError(apiError)))
      .finally(() => setUploading(false))
    analyzeReceiptFile(file)
  }

  const analyzeReceiptFile = (file = aiFile) => {
    if (!file) {
      setAiError({ message: 'Upload a receipt image before running LLM vision.' })
      return
    }

    setAiAnalyzing(true)
    setAiError(null)
    receiptManagementApi
      .analyzeReceiptImage(file)
      .then((analysis) => {
        setAiResult(analysis)
        applyAiAnalysis(analysis)
      })
      .catch((apiError) => setAiError(getApiError(apiError)))
      .finally(() => setAiAnalyzing(false))
  }

  const uploadImage = (event) => {
    uploadReceiptFile(event.target.files?.[0])
    event.target.value = ''
  }

  const preventImageDragDefault = (event) => {
    event.preventDefault()
    event.stopPropagation()
  }

  const handleImageDragEnter = (event) => {
    preventImageDragDefault(event)
    setDraggingImage(true)
  }

  const handleImageDragOver = (event) => {
    preventImageDragDefault(event)
    setDraggingImage(true)
  }

  const handleImageDragLeave = (event) => {
    preventImageDragDefault(event)
    setDraggingImage(false)
  }

  const handleImageDrop = (event) => {
    preventImageDragDefault(event)
    setDraggingImage(false)
    uploadReceiptFile(event.dataTransfer.files?.[0])
  }

  const applyAiAnalysis = (analysis = aiResult) => {
    if (!analysis) return

    const vendorId = findBestMatchId(analysis.vendorName, vendors, 'name', 'vendorId')
    const expenseCategoryId = findBestMatchId(analysis.categoryName, categories, 'name', 'expenseCategoryId')
    const detectedItems = buildAiItems(analysis)
    const paymentMethod = paymentMethods.includes(analysis.paymentMethod) ? analysis.paymentMethod : ''

    setForm((current) => ({
      ...current,
      receiptNumber: analysis.receiptNumber || current.receiptNumber,
      receiptDate: analysis.receiptDate || current.receiptDate,
      vendorId: vendorId || current.vendorId,
      expenseCategoryId: expenseCategoryId || current.expenseCategoryId,
      taxAmount: Number(analysis.taxAmount ?? current.taxAmount),
      paymentMethod: paymentMethod || current.paymentMethod,
      items: detectedItems.length ? detectedItems : current.items,
      notes: current.notes || buildAiNote(analysis),
    }))
  }

  const validate = () => {
    const errors = {}
    validateRequiredText(errors, 'receiptNumber', form.receiptNumber, 'Receipt number', validationLimits.receipt.receiptNumber)
    if (!form.receiptDate) errors.receiptDate = 'Receipt date is required.'
    validateRequiredSelection(errors, 'vendorId', form.vendorId, 'Vendor')
    validateRequiredSelection(errors, 'expenseCategoryId', form.expenseCategoryId, 'Expense category')
    validateNumberRange(errors, 'taxAmount', form.taxAmount, 'Tax amount', { min: 0, max: validationLimits.moneyMax })
    validateRequiredSelection(errors, 'paymentMethod', form.paymentMethod, 'Payment method')
    validateRequiredSelection(errors, 'status', form.status, 'Status')
    validateOptionalText(errors, 'notes', form.notes, 'Notes', validationLimits.receipt.notes)
    validateOptionalText(errors, 'imageUrl', form.imageUrl, 'Image URL', validationLimits.receipt.imageUrl)
    if (!form.items.length) errors.items = 'At least one item is required.'
    form.items.forEach((item, index) => {
      validateRequiredText(errors, `items.${index}.description`, item.description, `Item ${index + 1} description`, validationLimits.receipt.itemDescription)
      validateNumberRange(errors, `items.${index}.quantity`, item.quantity, `Item ${index + 1} quantity`, validationLimits.receipt.itemQuantity)
      validateNumberRange(errors, `items.${index}.unitPrice`, item.unitPrice, `Item ${index + 1} unit price`, validationLimits.receipt.itemUnitPrice)
      validateOptionalText(errors, `items.${index}.notes`, item.notes, `Item ${index + 1} notes`, validationLimits.receipt.itemNotes)
    })
    setClientErrors(errors)
    return Object.keys(errors).length === 0
  }

  const submit = (event) => {
    event.preventDefault()
    if (saving) return
    if (!validate()) return

    const payload = {
      ...form,
      vendorId: Number(form.vendorId),
      expenseCategoryId: Number(form.expenseCategoryId),
      taxAmount: Number(form.taxAmount),
      items: form.items.map((item) => ({
        description: item.description,
        quantity: Number(item.quantity),
        unitPrice: Number(item.unitPrice),
        notes: item.notes,
      })),
    }

    setSaving(true)
    const request = isEdit ? receiptManagementApi.updateReceipt(id, payload) : receiptManagementApi.createReceipt(payload)
    request
      .then(() => navigate('/receipts'))
      .catch((apiError) => {
        const normalizedError = getApiError(apiError)
        setError(normalizedError)
        const backendFieldErrors = mapBackendFieldErrors(normalizedError.errors)
        if (Object.keys(backendFieldErrors).length > 0) {
          setClientErrors((current) => ({ ...current, ...backendFieldErrors }))
        }
      })
      .finally(() => setSaving(false))
  }

  if (loading) {
    return <LoadingState label="Loading receipt..." />
  }

  return (
    <div className="space-y-6">
      <PageHeader eyebrow="Receipt Desk" title={isEdit ? 'Edit Receipt' : 'Create Receipt'} />
      <ErrorBanner error={error} />
      <ErrorBanner error={Object.keys(clientErrors).length ? { message: 'Fix highlighted frontend validation issues.', errors: clientErrors } : null} />

      <form className="grid gap-6 xl:grid-cols-[1fr_360px]" onSubmit={submit}>
        <div className="space-y-6">
          <ReceiptBasicFields
            categories={categories}
            errors={clientErrors}
            form={form}
            onFieldChange={updateField}
            vendors={vendors}
          />

          <Panel>
            <ReceiptItemsEditor errors={clientErrors} items={form.items} onChange={(items) => updateField('items', items)} />
          </Panel>
        </div>

        <div className="space-y-6 xl:sticky xl:top-8 xl:max-h-[calc(100vh-4rem)] xl:self-start xl:overflow-y-auto xl:pr-1">
          <ReceiptTotalsPanel totals={totals} />

          <ReceiptImagePanel
            draggingImage={draggingImage}
            imageUrl={form.imageUrl}
            onDragEnter={handleImageDragEnter}
            onDragLeave={handleImageDragLeave}
            onDragOver={handleImageDragOver}
            onDrop={handleImageDrop}
            onUpload={uploadImage}
            uploading={uploading}
          />

          <AiReceiptAssistPanel
            analyzing={aiAnalyzing}
            error={aiError}
            file={aiFile}
            onAnalyze={() => analyzeReceiptFile()}
            onApply={() => applyAiAnalysis()}
            result={aiResult}
          />

          <div className="flex flex-wrap justify-end gap-3">
            <PixelButton onClick={() => navigate('/receipts')} variant="ghost">Cancel</PixelButton>
            <PixelButton disabled={saving} icon={Save} type="submit">{saving ? 'Saving' : 'Save'}</PixelButton>
          </div>
        </div>
      </form>
    </div>
  )
}

function ReceiptBasicFields({ categories, errors, form, onFieldChange, vendors }) {
  return (
    <Panel className="grid gap-5 lg:grid-cols-2">
      <FormField error={errors.receiptNumber} label="Receipt Number">
        <input className={inputClass} onChange={(event) => onFieldChange('receiptNumber', event.target.value)} value={form.receiptNumber} />
      </FormField>
      <FormField error={errors.receiptDate} label="Date">
        <input className={inputClass} onChange={(event) => onFieldChange('receiptDate', event.target.value)} type="date" value={form.receiptDate} />
      </FormField>
      <FormField error={errors.vendorId} label="Vendor">
        <select className={selectClass} onChange={(event) => onFieldChange('vendorId', event.target.value)} value={form.vendorId}>
          <option value="">Select vendor</option>
          {vendors.map((vendor) => <option key={vendor.vendorId} value={vendor.vendorId}>{vendor.name}</option>)}
        </select>
      </FormField>
      <FormField error={errors.expenseCategoryId} label="Category">
        <select className={selectClass} onChange={(event) => onFieldChange('expenseCategoryId', event.target.value)} value={form.expenseCategoryId}>
          <option value="">Select category</option>
          {categories.map((category) => <option key={category.expenseCategoryId} value={category.expenseCategoryId}>{category.name}</option>)}
        </select>
      </FormField>
      <FormField error={errors.taxAmount} label="Tax Amount">
        <input className={inputClass} min="0" onChange={(event) => onFieldChange('taxAmount', event.target.value)} step="0.01" type="number" value={form.taxAmount} />
      </FormField>
      <FormField error={errors.paymentMethod} label="Payment Method">
        <select className={selectClass} onChange={(event) => onFieldChange('paymentMethod', event.target.value)} value={form.paymentMethod}>
          {paymentMethods.map((method) => <option key={method} value={method}>{method}</option>)}
        </select>
      </FormField>
      <FormField error={errors.status} label="Status">
        <select className={selectClass} onChange={(event) => onFieldChange('status', event.target.value)} value={form.status}>
          {receiptStatuses.map((status) => <option key={status} value={status}>{status}</option>)}
        </select>
      </FormField>
      <FormField error={errors.imageUrl} label="Image URL">
        <input className={inputClass} onChange={(event) => onFieldChange('imageUrl', event.target.value)} value={form.imageUrl} />
      </FormField>
      <div className="lg:col-span-2">
        <FormField error={errors.notes} label="Notes">
          <textarea className={textareaClass} onChange={(event) => onFieldChange('notes', event.target.value)} value={form.notes} />
        </FormField>
      </div>
    </Panel>
  )
}

function ReceiptTotalsPanel({ totals }) {
  return (
    <Panel className="space-y-4">
      <h2 className="font-title text-4xl font-bold leading-none text-ink">Totals</h2>
      <TotalRow label="Subtotal" value={totals.subtotalAmount} />
      <TotalRow label="Tax" value={totals.taxAmount} />
      <div className="rotate-[-0.4deg] rounded-[4px_10px_5px_12px/9px_4px_11px_5px] border-2 border-stamp-green bg-stamp-green/12 p-4 text-ink shadow-[2px_3px_0_rgba(74,156,106,0.22)]">
        <p className="text-sm leading-5 text-stamp-green">Auto Total</p>
        <p className="mt-2 font-title text-5xl font-bold leading-none">{formatCurrency(totals.totalAmount)}</p>
      </div>
    </Panel>
  )
}

function ReceiptImagePanel({
  draggingImage,
  imageUrl,
  onDragEnter,
  onDragLeave,
  onDragOver,
  onDrop,
  onUpload,
  uploading,
}) {
  return (
    <Panel className="space-y-4">
      <h2 className="font-title text-4xl font-bold leading-none text-ink">Receipt Image</h2>
      <div
        className={`relative grid min-h-52 place-items-center rounded-[3px_8px_4px_9px/7px_3px_9px_4px] border-2 border-dashed p-1 text-base leading-5 transition ${
          draggingImage
            ? 'border-tape-gold bg-tape-gold/12 text-ink shadow-[2px_3px_0_rgba(192,140,37,0.18)]'
            : 'border-ink/35 bg-paper-soft/70 text-ink-faint'
        }`}
        onDragEnter={onDragEnter}
        onDragLeave={onDragLeave}
        onDragOver={onDragOver}
        onDrop={onDrop}
      >
        {imageUrl ? (
          <img alt="Uploaded receipt" className="max-h-72 w-full rounded-[3px_8px_4px_9px/7px_3px_9px_4px] object-cover" src={resolveImageUrl(imageUrl)} />
        ) : (
          <span>{draggingImage ? 'Drop image to upload' : 'No image'}</span>
        )}
        {draggingImage && imageUrl ? (
          <div className="absolute inset-1 grid place-items-center rounded-[3px_8px_4px_9px/7px_3px_9px_4px] bg-paper-card/85 text-ink">
            Drop image to upload
          </div>
        ) : null}
      </div>
      <label className="inline-flex min-h-10 cursor-pointer items-center justify-center gap-2 rounded-[3px_9px_4px_11px/8px_3px_10px_4px] border-2 border-tape-gold bg-tape-gold/12 px-4 py-2 text-base leading-5 text-ink shadow-[2px_3px_0_rgba(192,140,37,0.18)] transition hover:-translate-y-0.5 hover:bg-tape-gold/20">
        <Upload size={16} />
        {uploading ? 'Uploading' : 'Upload Image'}
        <input accept={receiptImageRules.accept} className="hidden" disabled={uploading} onChange={onUpload} type="file" />
      </label>
    </Panel>
  )
}

function buildAiItems(analysis) {
  if (analysis.items?.length) {
    return analysis.items.map((item) => {
      const quantity = Number(item.quantity || 1)
      const lineTotal = Number(item.lineTotal || 0)
      const unitPrice = Number(item.unitPrice || (quantity > 0 ? lineTotal / quantity : 0))

      return {
        description: item.description || 'AI detected item',
        quantity: quantity > 0 ? quantity : 1,
        unitPrice: unitPrice >= 0 ? unitPrice.toFixed(2) : 0,
        notes: 'Generated from LLM vision. Please review before saving.',
      }
    })
  }

  const totalAmount = Number(analysis.totalAmount || 0)
  const taxAmount = Number(analysis.taxAmount || 0)
  if (totalAmount > 0) {
    return [{
      description: 'Receipt total before tax',
      quantity: 1,
      unitPrice: Math.max(totalAmount - taxAmount, 0).toFixed(2),
      notes: 'Generated from LLM vision. Please review before saving.',
    }]
  }

  return []
}

function buildAiNote(analysis) {
  const parts = []
  if (analysis.vendorName) parts.push(`LLM vendor: ${analysis.vendorName}`)
  if (analysis.categoryName) parts.push(`LLM category: ${analysis.categoryName}`)
  if (analysis.rawTextSummary) parts.push(`LLM summary: ${analysis.rawTextSummary}`)
  return parts.join('\n')
}

function findBestMatchId(value, records, labelKey, idKey) {
  const normalizedValue = normalizeMatchText(value)
  if (!normalizedValue) return ''

  const exact = records.find((record) => normalizeMatchText(record[labelKey]) === normalizedValue)
  if (exact) return exact[idKey]

  const partial = records.find((record) => {
    const normalizedRecord = normalizeMatchText(record[labelKey])
    return normalizedRecord.includes(normalizedValue) || normalizedValue.includes(normalizedRecord)
  })

  return partial?.[idKey] ?? ''
}

function normalizeMatchText(value) {
  return String(value ?? '').toLowerCase().replace(/[^a-z0-9]/g, '')
}

function TotalRow({ label, value }) {
  return (
    <div className="flex items-center justify-between gap-4 rounded-[3px_8px_4px_9px/7px_3px_9px_4px] border-2 border-dashed border-ink/30 bg-paper-soft/70 p-3 text-base leading-5">
      <span className="text-ink-muted">{label}</span>
      <span className="text-ink">{formatCurrency(value)}</span>
    </div>
  )
}

function clearFieldError(errors, field) {
  if (field !== 'items' && !errors[field]) {
    return errors
  }

  const nextErrors = { ...errors }
  delete nextErrors[field]
  if (field === 'items') {
    Object.keys(nextErrors).forEach((key) => {
      if (key.startsWith('items.')) {
        delete nextErrors[key]
      }
    })
  }
  return nextErrors
}

function mapBackendFieldErrors(errors) {
  if (!errors || typeof errors !== 'object') {
    return {}
  }

  return Object.entries(errors).reduce((fieldErrors, [field, messages]) => {
    const normalizedField = normalizeFieldName(field)
    fieldErrors[normalizedField] = Array.isArray(messages) ? messages.join(' ') : String(messages)
    return fieldErrors
  }, {})
}

function normalizeFieldName(field) {
  return String(field)
    .replace(/\[(\d+)\]/g, '.$1')
    .split('.')
    .map((part) => (part ? part.charAt(0).toLowerCase() + part.slice(1) : part))
    .join('.')
}
