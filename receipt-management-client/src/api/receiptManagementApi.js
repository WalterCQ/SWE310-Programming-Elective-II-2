import { httpClient } from './httpClient'

export const receiptManagementApi = {
  getVendors: () => httpClient.get('/vendors').then((response) => response.data.data ?? []),
  getVendor: (id) => httpClient.get(`/vendors/${id}`).then((response) => response.data.data),
  createVendor: (payload) => httpClient.post('/vendors', payload).then((response) => response.data.data),
  updateVendor: (id, payload) => httpClient.put(`/vendors/${id}`, payload).then((response) => response.data.data),
  deleteVendor: (id) => httpClient.delete(`/vendors/${id}`),

  getCategories: () => httpClient.get('/expense-categories').then((response) => response.data.data ?? []),
  getCategory: (id) => httpClient.get(`/expense-categories/${id}`).then((response) => response.data.data),
  createCategory: (payload) => httpClient.post('/expense-categories', payload).then((response) => response.data.data),
  updateCategory: (id, payload) => httpClient.put(`/expense-categories/${id}`, payload).then((response) => response.data.data),
  deleteCategory: (id) => httpClient.delete(`/expense-categories/${id}`),

  getReceipts: () => httpClient.get('/receipts').then((response) => response.data.data ?? []),
  getReceipt: (id) => httpClient.get(`/receipts/${id}`).then((response) => response.data.data),
  createReceipt: (payload) => httpClient.post('/receipts', payload).then((response) => response.data.data),
  updateReceipt: (id, payload) => httpClient.put(`/receipts/${id}`, payload).then((response) => response.data.data),
  deleteReceipt: (id) => httpClient.delete(`/receipts/${id}`),
  uploadReceiptImage: (file) => {
    const formData = new FormData()
    formData.append('file', file)
    return httpClient
      .post('/receipts/upload-image', formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      })
      .then((response) => response.data.data)
  },
  analyzeReceiptImage: (file) => {
    const formData = new FormData()
    formData.append('file', file)
    return httpClient
      .post('/receipts/analyze-image', formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      })
      .then((response) => response.data.data)
  },
}
