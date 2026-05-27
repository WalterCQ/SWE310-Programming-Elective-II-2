export function PageHeader({ eyebrow, title, actions }) {
  return (
    <div className="flex flex-col gap-4 border-b-2 border-dashed border-line pb-5 lg:flex-row lg:items-end lg:justify-between">
      <div className="space-y-3">
        <p className="text-base leading-5 tracking-[0.18em] text-tape-gold">{eyebrow}</p>
        <h1 className="max-w-5xl font-title text-5xl font-bold leading-none text-ink sm:text-6xl">{title}</h1>
      </div>
      {actions ? <div className="flex flex-wrap gap-3">{actions}</div> : null}
    </div>
  )
}
