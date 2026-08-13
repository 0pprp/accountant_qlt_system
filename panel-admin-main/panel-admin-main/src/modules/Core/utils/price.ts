export function formattedPrice(price: number): string {
  const cleanPrice = price.toString().replace(/,/g, '')

  return cleanPrice.replace(/\B(?=(\d{3})+(?!\d))/g, ',')
}
