import { receiptImageRules } from './constants'

export function validateRequiredText(errors, key, value, label, limits = {}) {
  const text = String(value ?? '')
  const trimmed = text.trim()

  if (!trimmed) {
    errors[key] = `${label} is required.`
    return
  }

  validateTextLength(errors, key, text, label, limits, true)
}

export function validateOptionalText(errors, key, value, label, limits = {}) {
  const text = String(value ?? '')
  if (!text) {
    return
  }

  validateTextLength(errors, key, text, label, limits, false)
}

export function validateNumberRange(errors, key, value, label, limits) {
  const number = Number(value)
  if (!Number.isFinite(number)) {
    errors[key] = `${label} must be a number.`
    return
  }

  if (number < limits.min) {
    errors[key] = `${label} must be at least ${limits.min}.`
    return
  }

  if (number > limits.max) {
    errors[key] = `${label} must be at most ${limits.max}.`
  }
}

export function validateEmail(errors, key, value, label, limits = {}) {
  validateOptionalText(errors, key, value, label, limits)
  if (errors[key] || !value) {
    return
  }

  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
    errors[key] = `${label} format is invalid.`
  }
}

export function validateHexColor(errors, key, value, label) {
  if (!/^#[0-9A-Fa-f]{6}$/.test(String(value ?? ''))) {
    errors[key] = `${label} must be a valid hex color such as #00F5FF.`
  }
}

export function validateRequiredSelection(errors, key, value, label) {
  if (!value) {
    errors[key] = `${label} is required.`
  }
}

export function validateReceiptImageFile(file) {
  if (!file) {
    return null
  }

  if (file.size > receiptImageRules.maxFileSizeBytes) {
    return `Receipt image must not exceed ${receiptImageRules.maxFileSizeLabel}.`
  }

  if (!receiptImageRules.allowedContentTypes.includes(file.type)) {
    return 'Only JPEG, PNG, and WEBP receipt images are allowed.'
  }

  const lowerName = file.name.toLowerCase()
  const hasAllowedExtension = receiptImageRules.allowedExtensions.some((extension) => lowerName.endsWith(extension))
  if (!hasAllowedExtension) {
    return 'The image file extension is not supported.'
  }

  return null
}

function validateTextLength(errors, key, value, label, limits, required) {
  const trimmedLength = value.trim().length
  if (required && limits.min && trimmedLength < limits.min) {
    errors[key] = `${label} must be at least ${limits.min} characters.`
    return
  }

  if (limits.max && value.length > limits.max) {
    errors[key] = `${label} must be ${limits.max} characters or fewer.`
  }
}
