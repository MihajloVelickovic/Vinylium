import type {AxiosInstance} from "axios";
import User from "../models/User.ts";
import {useFilteredList} from "./useFilteredList.ts";

export interface IUserFilters {
    pages: number;
    currentPage: number;
    items: number;
    search: string;
    admin: null | boolean;
}

const initialFilters: IUserFilters = {
    pages: 0,
    currentPage: 1,
    items: 20,
    search: "",
    admin: null
};

export const useUserFilters = (client: AxiosInstance) => {

    const {items: users, filters, setFilters, change, setChange, searchRef, loading, error} =
        useFilteredList<User, IUserFilters>(
            client,
            "/User/GetUsersFiltered",
            initialFilters,
            (f) => ({page: f.currentPage, items: f.items, search: f.search, admin: f.admin}),
            (raw) => new User(raw)
        );

    return {users, filters, setFilters, change, setChange, searchRef, loading, error};
}
