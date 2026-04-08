import "../styles/Filters.css"

export const Filters = () => {
    return (
        <div className="filters">
            <input type="text" placeholder="Title"></input>
            <input type="text" placeholder="Artist"></input>
            <select>
                <option>Vinyl</option>
                <option>CD</option>
                <option>Cassette</option>
            </select>
            <input placeholder="Price from"></input>
            <input placeholder="Price to"></input> 
        </div>
    )
}