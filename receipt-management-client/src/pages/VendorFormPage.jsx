import { Save } from 'lucide-react'
import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { getApiError } from '../api/httpClient'
import { receiptManagementApi } from '../api/receiptManagementApi'
import { ErrorBanner } from '../components/ErrorBanner'
import { FormField, inputClass, textareaClass } from '../components/FormField'
import { LoadingState } from '../components/LoadingState'
import { PageHeader } from '../components/PageHeader'
import { Panel } from '../components/Panel'
import { PixelButton } from '../components/PixelButton'
import { validationLimits } from '../utils/constants'
import { validateEmail, validateOptionalText, validateRequiredText } from '../utils/validation'

const emptyVendor = {
  name: '',
  contactPerson: '',
  phone: '',
  email: '',
  address: '',
  taxRegistrationNumber: '',
  notes: '',
}

export function VendorFormPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const isEdit = Boolean(id)
  const [form, setForm] = useState(emptyVendor)
  const [loading, setLoading] = useState(isEdit)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState(null)
  const [clientErrors, setClientErrors] = useState({})

  useEffect(() => {
    if (!isEdit) {
      return
    }

    receiptManagementApi
      .getVendor(id)
      .then((vendor) => {
        setForm({
          name: vendor.name ?? '',
          contactPerson: vendor.contactPerson ?? '',
          phone: vendor.phone ?? '',
          email: vendor.email ?? '',
          address: vendor.address ?? '',
          taxRegistrationNumber: vendor.taxRegistrationNumber ?? '',
          notes: vendor.notes ?? '',
        })
      })
      .catch((apiError) => setError(getApiError(apiError)))
      .finally(() => setLoading(false))
  }, [id, isEdit])

  const updateField = (field, value) => setForm((current) => ({ ...current, [field]: value }))

  const validate = () => {
    const errors = {}
    validateRequiredText(errors, 'name', form.name, 'Vendor name', validationLimits.vendor.name)
    validateOptionalText(errors, 'contactPerson', form.contactPerson, 'Contact person', validationLimits.vendor.contactPerson)
    validateOptionalText(errors, 'phone', form.phone, 'Phone', validationLimits.vendor.phone)
    validateEmail(errors, 'email', form.email, 'Email', validationLimits.vendor.email)
    validateOptionalText(errors, 'address', form.address, 'Address', validationLimits.vendor.address)
    validateOptionalText(errors, 'taxRegistrationNumber', form.taxRegistrationNumber, 'Tax registration number', validationLimits.vendor.taxRegistrationNumber)
    validateOptionalText(errors, 'notes', form.notes, 'Notes', validationLimits.vendor.notes)
    setClientErrors(errors)
    return Object.keys(errors).length === 0
  }

  const submit = (event) => {
    event.preventDefault()
    if (!validate()) return

    setSaving(true)
    const request = isEdit ? receiptManagementApi.updateVendor(id, form) : receiptManagementApi.createVendor(form)
    request
      .then(() => navigate('/vendors'))
      .catch((apiError) => setError(getApiError(apiError)))
      .finally(() => setSaving(false))
  }

  if (loading) {
    return <LoadingState label="Loading vendor..." />
  }

  return (
    <div className="space-y-6">
      <PageHeader eyebrow="Vendor Notebook" title={isEdit ? 'Edit Vendor' : 'Create Vendor'} />
      <ErrorBanner error={error} />
      <Panel>
        <form className="grid gap-5" onSubmit={submit}>
          <div className="grid gap-5 lg:grid-cols-2">
            <FormField error={clientErrors.name} label="Vendor Name">
              <input className={inputClass} onChange={(event) => updateField('name', event.target.value)} value={form.name} />
            </FormField>
            <FormField error={clientErrors.contactPerson} label="Contact Person">
              <input className={inputClass} onChange={(event) => updateField('contactPerson', event.target.value)} value={form.contactPerson} />
            </FormField>
            <FormField error={clientErrors.phone} label="Phone">
              <input className={inputClass} onChange={(event) => updateField('phone', event.target.value)} value={form.phone} />
            </FormField>
            <FormField error={clientErrors.email} label="Email">
              <input className={inputClass} onChange={(event) => updateField('email', event.target.value)} value={form.email} />
            </FormField>
            <FormField error={clientErrors.taxRegistrationNumber} label="Tax Registration Number">
              <input className={inputClass} onChange={(event) => updateField('taxRegistrationNumber', event.target.value)} value={form.taxRegistrationNumber} />
            </FormField>
          </div>
          <FormField error={clientErrors.address} label="Address">
            <textarea className={textareaClass} onChange={(event) => updateField('address', event.target.value)} value={form.address} />
          </FormField>
          <FormField error={clientErrors.notes} label="Notes">
            <textarea className={textareaClass} onChange={(event) => updateField('notes', event.target.value)} value={form.notes} />
          </FormField>
          <div className="flex justify-end gap-3">
            <PixelButton onClick={() => navigate('/vendors')} variant="ghost">Cancel</PixelButton>
            <PixelButton icon={Save} variant="primary" onClick={submit}>{saving ? 'Saving' : 'Save'}</PixelButton>
          </div>
        </form>
      </Panel>
    </div>
  )
}
