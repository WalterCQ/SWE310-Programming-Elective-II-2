import { Plus, Trash2 } from 'lucide-react'
import { calculateLineTotal, formatCurrency } from '../utils/formatters'
import { FormField, inputClass } from './FormField'
import { PixelButton } from './PixelButton'

export function ReceiptItemsEditor({ items, onChange, errors = {} }) {
  const updateItem = (index, field, value) => {
    const nextItems = items.map((item, itemIndex) => (itemIndex === index ? { ...item, [field]: value } : item))
    onChange(nextItems)
  }

  const addItem = () => {
    onChange([...items, { description: '', quantity: 1, unitPrice: 0, notes: '' }])
  }

  const removeItem = (index) => {
    onChange(items.filter((_, itemIndex) => itemIndex !== index))
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h2 className="font-title text-4xl font-bold leading-none text-ink">Receipt Items</h2>
        <PixelButton icon={Plus} onClick={addItem} variant="amber">
          Add Item
        </PixelButton>
      </div>

      <div className="space-y-4">
        {items.map((item, index) => (
          <div className="rounded-[4px_10px_5px_12px/9px_4px_11px_5px] border-2 border-dashed border-ink/35 bg-paper-soft/70 p-4" key={index}>
            <div className="grid gap-4 lg:grid-cols-[2fr_1fr_1fr_1fr_auto]">
              <FormField error={errors[`items.${index}.description`]} label="Description">
                <input
                  className={inputClass}
                  onChange={(event) => updateItem(index, 'description', event.target.value)}
                  placeholder="Taxi fare"
                  value={item.description}
                />
              </FormField>
              <FormField error={errors[`items.${index}.quantity`]} label="Quantity">
                <input
                  className={inputClass}
                  min="0.01"
                  onChange={(event) => updateItem(index, 'quantity', event.target.value)}
                  step="0.01"
                  type="number"
                  value={item.quantity}
                />
              </FormField>
              <FormField error={errors[`items.${index}.unitPrice`]} label="Unit Price">
                <input
                  className={inputClass}
                  min="0"
                  onChange={(event) => updateItem(index, 'unitPrice', event.target.value)}
                  step="0.01"
                  type="number"
                  value={item.unitPrice}
                />
              </FormField>
              <div className="space-y-2">
                <p className="text-base leading-5 text-ink-muted">Line Total</p>
                <div className="rounded-[3px_8px_4px_9px/7px_3px_9px_4px] border-2 border-stamp-green/70 bg-stamp-green/10 px-3 py-2.5 text-lg leading-6 text-ink">
                  {formatCurrency(calculateLineTotal(item))}
                </div>
              </div>
              <div className="flex items-end">
                <button
                  className="min-h-12 rounded-[3px_8px_4px_9px/7px_3px_9px_4px] border-2 border-pencil-red/70 px-3 text-pencil-red hover:bg-pencil-red hover:text-paper-card"
                  onClick={() => removeItem(index)}
                  title="Remove item"
                  type="button"
                >
                  <Trash2 size={18} />
                </button>
              </div>
            </div>
            <div className="mt-4">
              <FormField error={errors[`items.${index}.notes`]} label="Notes">
                <input
                  className={inputClass}
                  onChange={(event) => updateItem(index, 'notes', event.target.value)}
                  placeholder="Optional item notes"
                  value={item.notes ?? ''}
                />
              </FormField>
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}
