import client from "./client";

export interface RegisterRequest{
    email: string;
    username: string;
    password: string;
}

export const registerUser = (data: RegisterRequest) => 
    client.post("/User/Register", data);