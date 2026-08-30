import "../styles/Filters.css"
// import Search from "../assets/search.svg"

export const Filters = ({searchRef, params, children}) => {

    return (
        <div className="filtersFull">
            <div>
                <form className="filters" onSubmit={(e) => {
                    // e.preventDefault()
                    // params.handleSearch().then(res => setProducts(res)).catch(err => console.log(err));
                }}
                    >
                    <label>Per Page</label>
                    <select onChange={(t) => {
                        // 0 because selectedOptions only has one item (no 'multiple' tag)
                        params.setFilters({...params.filters, items:parseInt(t.target.selectedOptions[0].innerText)})
                        params.setChange(!params.change)
                    }}>
                        {
                            //TODO remove literals
                            [20, 30, 40].map((item) => {
                                return <option>{item}</option>
                            })
                        }
                    </select>
                    <input ref={searchRef} type="text" placeholder="Search"
                    onInput={(e) => {
                        params.setFilters({...params.filters, search: e.currentTarget.value})
                        params.setChange(!params.change)
                    }}></input>
                    {children}

                    {/*<input type="image" alt="Search" src={Search}></input>*/}

                </form>
            </div>
            <div className="pages">
                {
                    Array.from(Array(params.filters.pages).keys()).map((_, i) => (i+1)).map((n => {
                    return <button onClick={() => {
                        params.setFilters({...params.filters, currentPage: n})
                        params.setChange(!params.change)
                    }} className={(params.filters.currentPage == n ? "clicked" : "")+" button-main"}>{n}</button>
                }))
                }
            </div>
        </div>
    )
}
