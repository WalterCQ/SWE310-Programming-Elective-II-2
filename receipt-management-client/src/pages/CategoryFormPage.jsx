import { ChevronDown, Save } from 'lucide-react'
import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { getApiError } from '../api/httpClient'
import { receiptManagementApi } from '../api/receiptManagementApi'
import { CategoryIcon } from '../components/CategoryIcon'
import { ErrorBanner } from '../components/ErrorBanner'
import { FormField, inputClass, selectClass, textareaClass } from '../components/FormField'
import { LoadingState } from '../components/LoadingState'
import { PageHeader } from '../components/PageHeader'
import { Panel } from '../components/Panel'
import { PixelButton } from '../components/PixelButton'
import { categoryIcons, validationLimits } from '../utils/constants'
import { validateHexColor, validateNumberRange, validateOptionalText, validateRequiredText } from '../utils/validation'

const emptyCategory = {
  name: '',
  description: '',
  monthlyBudget: 0,
  colorHex: '#4A9C6A',
  iconName: 'receipt',
}

export function CategoryFormPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const isEdit = Boolean(id)
  const [form, setForm] = useState(emptyCategory)
  const [loading, setLoading] = useState(isEdit)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState(null)
  const [clientErrors, setClientErrors] = useState({})

  useEffect(() => {
    if (!isEdit) return

    receiptManagementApi
      .getCategory(id)
      .then((category) => setForm({
        name: category.name ?? '',
        description: category.description ?? '',
        monthlyBudget: category.monthlyBudget ?? 0,
        colorHex: category.colorHex ?? '#4A9C6A',
        iconName: category.iconName ?? 'receipt',
      }))
      .catch((apiError) => setError(getApiError(apiError)))
      .finally(() => setLoading(false))
  }, [id, isEdit])

  const updateField = (field, value) => setForm((current) => ({ ...current, [field]: value }))

  const validate = () => {
    const errors = {}
    validateRequiredText(errors, 'name', form.name, 'Category name', validationLimits.category.name)
    validateOptionalText(errors, 'description', form.description, 'Description', validationLimits.category.description)
    validateNumberRange(errors, 'monthlyBudget', form.monthlyBudget, 'Monthly budget', { min: 0, max: validationLimits.moneyMax })
    validateHexColor(errors, 'colorHex', form.colorHex, 'Color hex')
    validateRequiredText(errors, 'iconName', form.iconName, 'Icon name', validationLimits.category.iconName)
    setClientErrors(errors)
    return Object.keys(errors).length === 0
  }

  const submit = (event) => {
    event.preventDefault()
    if (!validate()) return

    setSaving(true)
    const payload = { ...form, monthlyBudget: Number(form.monthlyBudget) }
    const request = isEdit ? receiptManagementApi.updateCategory(id, payload) : receiptManagementApi.createCategory(payload)
    request
      .then(() => navigate('/categories'))
      .catch((apiError) => setError(getApiError(apiError)))
      .finally(() => setSaving(false))
  }

  if (loading) {
    return <LoadingState label="Loading category..." />
  }

  return (
    <div className="space-y-6">
      <PageHeader eyebrow="category notebook" title={isEdit ? 'Edit Category' : 'Create Category'} />
      <ErrorBanner error={error} />
      <Panel>
        <form className="grid gap-5" onSubmit={submit}>
          <div className="grid gap-5 lg:grid-cols-2">
            <FormField error={clientErrors.name} label="Name">
              <input className={inputClass} onChange={(event) => updateField('name', event.target.value)} value={form.name} />
            </FormField>
            <FormField error={clientErrors.monthlyBudget} label="Monthly Budget">
              <input className={inputClass} min="0" onChange={(event) => updateField('monthlyBudget', event.target.value)} step="0.01" type="number" value={form.monthlyBudget} />
            </FormField>
            <FormField error={clientErrors.colorHex} label="Color Hex">
              <input className={inputClass} onChange={(event) => updateField('colorHex', event.target.value)} value={form.colorHex} />
            </FormField>
            <FormField error={clientErrors.iconName} label="Icon Name">
              <div className="flex items-center gap-3">
                <CategoryIcon className="h-11 w-11" color={form.colorHex} iconName={form.iconName} label={`${form.iconName} preview`} />
                <div className="relative min-w-0 flex-1">
                  <select className={`${selectClass} cursor-pointer pr-11`} onChange={(event) => updateField('iconName', event.target.value)} value={form.iconName}>
                    {categoryIcons.map((icon) => <option key={icon} value={icon}>{icon}</option>)}
                  </select>
                  <ChevronDown aria-hidden="true" className="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 text-ink-muted" size={18} />
                </div>
              </div>
            </FormField>
          </div>
          <FormField error={clientErrors.description} label="Description">
            <textarea className={textareaClass} onChange={(event) => updateField('description', event.target.value)} value={form.description} />
          </FormField>
          <div className="flex justify-end gap-3">
            <PixelButton onClick={() => navigate('/categories')} variant="ghost">Cancel</PixelButton>
            <PixelButton icon={Save} onClick={submit}>{saving ? 'Saving' : 'Save'}</PixelButton>
          </div>
        </form>
      </Panel>
    </div>
  )
}
