export const UserFilterFields = ({params}) => {
    return (
        <select onChange={(t) => {
            const idx = t.target.selectedIndex;
            params.setFilters({...params.filters, admin: idx <= 0 ? null : idx === 1})
            params.setChange(!params.change)
        }}>
            <option selected>Role</option>
            <option>Admin</option>
            <option>User</option>
        </select>
    )
}
