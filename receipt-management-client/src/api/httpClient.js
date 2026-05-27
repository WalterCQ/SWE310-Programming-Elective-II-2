import axios from 'axios'

export const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5068/api'
export const apiOrigin = apiBaseUrl.replace(/\/api\/?$/, '')

export const httpClient = axios.create({
  baseURL: apiBaseUrl,
  headers: {
    'Content-Type': 'application/json',
  },
})

export function resolveImageUrl(imageUrl) {
  if (!imageUrl) {
    return ''
  }

  if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
    return imageUrl
  }

  return `${apiOrigin}${imageUrl}`
}

export function getApiError(error) {
  const payload = error?.response?.data

  if (payload?.errors) {
    return {
      message: payload.message ?? 'The request failed.',
      errors: payload.errors,
    }
  }

  return {
    message: payload?.message ?? error?.message ?? 'The request failed.',
    errors: null,
  }
}
