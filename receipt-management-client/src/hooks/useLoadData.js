import { useEffect, useState } from 'react'
import { getApiError } from '../api/httpClient'

export function useLoadData(loader, initialData) {
  const [data, setData] = useState(initialData)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [reloadToken, setReloadToken] = useState(0)

  useEffect(() => {
    let isMounted = true

    loader()
      .then((loadedData) => {
        if (!isMounted) return
        setData(loadedData)
        setError(null)
      })
      .catch((apiError) => {
        if (!isMounted) return
        setError(getApiError(apiError))
      })
      .finally(() => {
        if (isMounted) setLoading(false)
      })

    return () => {
      isMounted = false
    }
  }, [loader, reloadToken])

  return {
    data,
    error,
    loading,
    reload: () => {
      setLoading(true)
      setReloadToken((current) => current + 1)
    },
    setError,
  }
}
