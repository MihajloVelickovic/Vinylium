import type {AxiosInstance} from "axios";
import Store from "../models/Store.ts";
import {useFilteredList} from "./useFilteredList.ts";

export interface IStoreFilters {
    pages: number;
    currentPage: number;
    items: number;
    search: string;
    isWarehouse: null | boolean;
}

const initialFilters: IStoreFilters = {
    pages: 0,
    currentPage: 1,
    items: 20,
    search: "",
    isWarehouse: null
};

export const useStoreFilters = (client: AxiosInstance) => {

    const {items: stores, filters, setFilters, change, setChange, searchRef, loading, error} =
        useFilteredList<Store, IStoreFilters>(
            client,
            "/Store/GetStoresFiltered",
            initialFilters,
            (f) => ({page: f.currentPage, items: f.items, search: f.search, isWarehouse: f.isWarehouse}),
            (raw) => new Store(raw)
        );

    return {stores, filters, setFilters, change, setChange, searchRef, loading, error};
}
