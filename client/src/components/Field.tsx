import type {InputHTMLAttributes, ReactNode, Ref} from "react";

type FieldProps = {
    label: string;
    value: string | null;
    onChange: (value: string) => void;
    inputRef?: Ref<HTMLInputElement>;
    rowClassName?: string;
    testId?: string;
    children?: ReactNode;
} & Omit<InputHTMLAttributes<HTMLInputElement>, "value" | "onChange" | "children">;

export const Field = ({label, value, onChange, inputRef, rowClassName, testId, children, type, ...rest}: FieldProps) => (
    <div className={"productInput textBord" + (rowClassName ? " " + rowClassName : "")}>
        <p>{label}</p>
        <input {...rest}
               className="iField"
               data-testid={testId}
               type={type ?? "text"}
               spellCheck={false}
               value={value ?? ""}
               onChange={e => onChange(e.target.value)}
               ref={inputRef}/>
        {children}
    </div>
);
