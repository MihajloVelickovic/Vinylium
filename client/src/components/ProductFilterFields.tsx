import Product from "../models/Product";

export const ProductFilterFields = ({params}) => {
    return (
        <>
            <select data-testid="filter-type" onChange={(t) => {
                console.log(t.target.selectedIndex)
                params.setFilters({...params.filters, type: t.target.selectedIndex <= 0 ? null : t.target.selectedIndex-1})
                params.setChange(!params.change)
            }}>
                <option selected>Type</option>
                {
                    //TODO remove literals
                    [0, 1, 2].map((item) => {
                        return <option>{Product.evaluateType(item)}</option>
                    })
                }
            </select>
            <input data-testid="filter-price-low" placeholder="Price from" type="number"
            onInput={(e) => {
                params.setFilters({...params.filters, priceLow: e.currentTarget.value})
                params.setChange(!params.change)
            }}></input>
            <input data-testid="filter-price-high" placeholder="Price to" type="number"
            onInput={(e) => {
                params.setFilters({...params.filters, priceHigh: e.currentTarget.value})
                params.setChange(!params.change)
            }}></input>
        </>
    )
}
