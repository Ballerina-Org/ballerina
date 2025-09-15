import axios, {AxiosHeaders, AxiosError, AxiosRequestConfig} from "axios";
import { test } from "../domains/storage/local";
import {ValueOrErrors} from "ballerina-core";
import {List} from "immutable";
export const api = axios.create({ baseURL: process.env.API_PREFIX ?? "/api/preview" });

api.interceptors.request.use((cfg) => {
    const t = test.get();
    if (!t) return cfg;
    
    cfg.headers = AxiosHeaders.from(cfg.headers);
    (cfg.headers as AxiosHeaders).set("Test-Dummy-Header", t);

    return cfg;
});


export async function axiosVOE<T>(
    config: AxiosRequestConfig
): Promise<ValueOrErrors<T, string>> {

    try {
        const res = await api.request<T>({
            validateStatus: () => true,
            headers: { Accept: "application/json", ...(config.headers || {}) },
            ...config,
        });
    
        if (res.status >= 200 && res.status < 300) {
            return ValueOrErrors.Default.return(res.data as T )
        }
    
        const data: any = res.data;
        const merged = Array.from(
            new Set(
                [
                    ...(Array.isArray(data?.errors) ? data.errors.map(String) : []), 
                    typeof data?.title === "string" ? data.title : undefined,       
                    typeof data?.detail === "string" ? data.detail : undefined,     
                    res.statusText || "Unknown error",
                ].filter(Boolean) as string[]
            )
        );
        
        return ValueOrErrors.Default.throw(List(merged));

    } catch (err) {
        const ax = err as AxiosError;
        const msg =
            (ax.message && String(ax.message)) ||
            (typeof err === "string" ? err : "Network error");
        return ValueOrErrors.Default.throwOne(msg);
    }

}