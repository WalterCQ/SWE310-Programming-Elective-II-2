import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AppShell } from './components/AppShell'
import { CategoriesPage } from './pages/CategoriesPage'
import { CategoryFormPage } from './pages/CategoryFormPage'
import { DashboardPage } from './pages/DashboardPage'
import { ReceiptFormPage } from './pages/ReceiptFormPage'
import { ReceiptsPage } from './pages/ReceiptsPage'
import { VendorFormPage } from './pages/VendorFormPage'
import { VendorsPage } from './pages/VendorsPage'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<AppShell />}>
          <Route element={<DashboardPage />} index />
          <Route element={<ReceiptsPage />} path="receipts" />
          <Route element={<ReceiptFormPage />} path="receipts/new" />
          <Route element={<ReceiptFormPage />} path="receipts/:id/edit" />
          <Route element={<VendorsPage />} path="vendors" />
          <Route element={<VendorFormPage />} path="vendors/new" />
          <Route element={<VendorFormPage />} path="vendors/:id/edit" />
          <Route element={<CategoriesPage />} path="categories" />
          <Route element={<CategoryFormPage />} path="categories/new" />
          <Route element={<CategoryFormPage />} path="categories/:id/edit" />
          <Route element={<Navigate to="/" replace />} path="*" />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}

export default App
