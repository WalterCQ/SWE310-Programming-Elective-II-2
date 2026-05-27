import { Link } from 'react-router-dom'

const variants = {
  primary: 'border-ink bg-paper-card text-ink shadow-[2px_3px_0_rgba(27,37,64,0.20)] hover:border-stamp-green hover:bg-stamp-green/10 hover:text-stamp-green',
  danger: 'border-pencil-red bg-pencil-red/10 text-pencil-red shadow-[2px_3px_0_rgba(214,57,74,0.18)] hover:bg-pencil-red hover:text-paper-card',
  ghost: 'border-ink-muted/60 bg-transparent text-ink-muted hover:border-ink hover:bg-ink/5 hover:text-ink',
  amber: 'border-tape-gold bg-tape-gold/12 text-ink shadow-[2px_3px_0_rgba(192,140,37,0.18)] hover:bg-tape-gold/20 hover:text-tape-gold',
}

export function PixelButton({ children, to, variant = 'primary', icon: Icon, className = '', ...props }) {
  const classes = `inline-flex min-h-10 items-center justify-center gap-2 rounded-[3px_9px_4px_11px/8px_3px_10px_4px] border-2 px-4 py-2 text-base leading-5 tracking-wide transition duration-150 hover:-translate-y-0.5 hover:rotate-[-0.3deg] active:translate-x-0.5 active:translate-y-0.5 active:shadow-none disabled:cursor-not-allowed disabled:opacity-60 disabled:hover:translate-y-0 disabled:hover:rotate-0 ${variants[variant]} ${className}`

  const content = (
    <>
      {Icon ? <Icon size={16} strokeWidth={2.5} /> : null}
      <span>{children}</span>
    </>
  )

  if (to) {
    return (
      <Link className={classes} to={to}>
        {content}
      </Link>
    )
  }

  return (
    <button className={classes} type="button" {...props}>
      {content}
    </button>
  )
}
