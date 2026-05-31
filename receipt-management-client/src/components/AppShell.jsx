import { LayoutDashboard, ReceiptText, Store, Tags } from 'lucide-react'
import { NavLink, Outlet } from 'react-router-dom'

const navItems = [
  { to: '/', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/receipts', label: 'Receipts', icon: ReceiptText },
  { to: '/vendors', label: 'Vendors', icon: Store },
  { to: '/categories', label: 'Categories', icon: Tags },
]

export function AppShell() {
  return (
    <div className="min-h-screen overflow-hidden bg-paper font-hand text-ink">
      <div className="fixed inset-0 -z-10 bg-[radial-gradient(circle_at_1px_1px,rgba(75,110,165,0.13)_1px,transparent_1.35px),repeating-linear-gradient(0deg,transparent_0px,transparent_119px,rgba(75,110,165,0.045)_119px,rgba(75,110,165,0.045)_120px)] bg-[length:24px_24px,100%_120px]" />
      <div className="pointer-events-none fixed inset-0 -z-10 bg-[radial-gradient(ellipse_640px_420px_at_8%_4%,rgba(214,57,74,0.06),transparent_60%),radial-gradient(ellipse_760px_480px_at_92%_96%,rgba(27,37,64,0.045),transparent_62%)]" />
      <div className="pointer-events-none fixed inset-0 -z-10 opacity-[0.18] mix-blend-multiply bg-[url('data:image/svg+xml,%3Csvg_xmlns=%22http://www.w3.org/2000/svg%22_width=%22240%22_height=%22240%22%3E%3Cfilter_id=%22n%22%3E%3CfeTurbulence_type=%22fractalNoise%22_baseFrequency=%220.9%22_numOctaves=%222%22_stitchTiles=%22stitch%22/%3E%3C/filter%3E%3Crect_width=%22100%25%22_height=%22100%25%22_filter=%22url(%23n)%22_opacity=%220.45%22/%3E%3C/svg%3E')]" />

      <aside className="fixed inset-x-0 top-0 z-30 border-b-2 border-dashed border-line bg-paper/95 px-4 py-3 shadow-[0_10px_28px_rgba(27,37,64,0.08)] backdrop-blur-sm lg:inset-y-0 lg:left-0 lg:right-auto lg:w-72 lg:border-b-0 lg:border-r-2 lg:px-5">
        <div className="flex items-center justify-between gap-4 lg:block">
          <div>
            <p className="text-sm leading-5 tracking-[0.22em] text-tape-gold">Pocket Ledger</p>
            <h1 className="mt-1 font-title text-4xl font-bold leading-none text-ink">Receipt Book</h1>
          </div>
          <div className="hidden rotate-[-1deg] rounded-[3px_8px_4px_10px/8px_3px_9px_4px] border-2 border-stamp-green/70 bg-stamp-green/10 px-3 py-2 text-sm leading-5 text-stamp-green sm:block lg:mt-5 lg:inline-block">
            grocery runs, coffee stops, little claims～
          </div>
        </div>

        <nav className="mt-4 flex gap-2 overflow-x-auto pb-1 lg:mt-8 lg:block lg:space-y-3 lg:overflow-visible lg:pb-0">
          {navItems.map((item) => (
            <NavLink
              className={({ isActive }) =>
                `flex min-w-max items-center gap-3 rounded-[3px_9px_4px_11px/8px_3px_10px_4px] border-2 px-3 py-3 text-base leading-5 transition hover:-translate-y-0.5 ${
                  isActive
                    ? 'border-ink bg-paper-card text-ink shadow-[2px_3px_0_rgba(27,37,64,0.22)]'
                    : 'border-ink/35 bg-paper-soft/70 text-ink-muted hover:border-ink hover:bg-paper-card hover:text-ink'
                }`
              }
              key={item.to}
              to={item.to}
              end={item.to === '/'}
            >
              <item.icon size={16} strokeWidth={2.5} />
              {item.label}
            </NavLink>
          ))}
        </nav>
      </aside>

      <main className="px-4 pb-10 pt-44 sm:px-6 lg:ml-72 lg:px-8 lg:pt-8">
        <div className="mx-auto max-w-7xl">
          <Outlet />
        </div>
      </main>
    </div>
  )
}
