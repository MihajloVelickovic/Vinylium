export const StoreFilterFields = ({params}) => {
    return (
        <select onChange={(t) => {
            const idx = t.target.selectedIndex;
            params.setFilters({...params.filters, isWarehouse: idx <= 0 ? null : idx === 1})
            params.setChange(!params.change)
        }}>
            <option selected>Warehouse</option>
            <option>Yes</option>
            <option>No</option>
        </select>
    )
}
