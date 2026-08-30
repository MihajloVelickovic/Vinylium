import {useCallback, useEffect, useRef, useState} from "react";
import type {AxiosInstance} from "axios";

export interface IBaseFilters {
    pages: number;
    currentPage: number;
    items: number;
    search: string;
}

export const useFilteredList = <TItem, TFilters extends IBaseFilters>(
    client: AxiosInstance,
    endpoint: string,
    initialFilters: TFilters,
    buildParams: (filters: TFilters) => Record<string, unknown>,
    deserialize: (raw: any) => TItem
) => {

    const [items, setItems] = useState<TItem[]>([]);
    const [change, setChange] = useState(false);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");
    const searchRef = useRef<HTMLInputElement>(null);

    const [filters, setFilters] = useState<TFilters>(initialFilters);

    useEffect(() => {
        /* debouncing
         * sets a timer to execute the query, but resets it if its
         * called again before the timer runs out, and discards the
         * call that was supposed to happen, and instead starts a new timer
         */
        const timer = setTimeout(() =>
                fetchItems().then(res => {
                    finalizeData(res);
                })
                    .catch(err => {
                        console.log(err);
                        setError(err?.response?.data?.message ?? "Failed to load data");
                    })
                    .finally(() => setLoading(false))
            , 100)
        return () => clearTimeout(timer);
    }, [change]);

    useEffect(() => {
        window.addEventListener("keydown", handleKeyPress);
        return () => {
            window.removeEventListener("keydown", handleKeyPress);
        }
    }, []);

    const handleKeyPress = useCallback((e) => {

        const currentElement = document.activeElement;
        /* forward slash and division */
        if ((e.which === 111 || e.which == 191) && document.activeElement !== searchRef.current
            && !(currentElement instanceof HTMLInputElement)) {
            e.preventDefault();
            searchRef.current!.focus();
        }

        /* escape */
        if (e.which === 27 && currentElement instanceof HTMLInputElement) {
            e.preventDefault();
            currentElement.blur();
        }
    }, [])

    const fetchItems = async () => {
        return await client.get(endpoint, {params: buildParams(filters)});
    }

    const finalizeData = (result: any) => {
        setFilters({...filters, pages: result.data.pages, currentPage: 1});
        setItems(result.data.data.map((raw: any) => deserialize(raw)));
    }

    return {items, filters, setFilters, change, setChange, searchRef, loading, error};
}
