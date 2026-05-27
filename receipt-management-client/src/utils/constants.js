export const paymentMethods = ['Cash', 'CreditCard', 'DebitCard', 'EWallet', 'BankTransfer']
export const receiptStatuses = ['Draft', 'Recorded', 'Reimbursed', 'Archived']
export const currencyCode = 'MYR'

export const categoryIcons = [
  'utensils',
  'car',
  'briefcase',
  'shopping-bag',
  'zap',
  'plane',
  'monitor',
  'heart-pulse',
  'ticket',
  'graduation-cap',
  'receipt',
]

export const ledgerColors = ['#1B2540', '#D6394A', '#4A9C6A', '#C08C25', '#7D5CA6', '#6B7693', '#B06B3A', '#2A6F8F']

export const validationLimits = {
  moneyMax: 999999.99,
  receipt: {
    receiptNumber: { min: 2, max: 40 },
    notes: { max: 500 },
    imageUrl: { max: 300 },
    itemDescription: { min: 2, max: 160 },
    itemQuantity: { min: 0.01, max: 99999.99 },
    itemUnitPrice: { min: 0, max: 999999.99 },
    itemNotes: { max: 250 },
  },
  vendor: {
    name: { min: 2, max: 120 },
    contactPerson: { max: 100 },
    phone: { max: 30 },
    email: { max: 120 },
    address: { max: 250 },
    taxRegistrationNumber: { max: 60 },
    notes: { max: 300 },
  },
  category: {
    name: { min: 2, max: 80 },
    description: { max: 250 },
    iconName: { max: 40 },
  },
}

export const receiptImageRules = {
  maxFileSizeBytes: 5 * 1024 * 1024,
  maxFileSizeLabel: '5 MB',
  allowedContentTypes: ['image/jpeg', 'image/png', 'image/webp'],
  allowedExtensions: ['.jpg', '.jpeg', '.png', '.webp'],
  accept: 'image/png,image/jpeg,image/webp',
}
