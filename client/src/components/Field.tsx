import type {InputHTMLAttributes, ReactNode, Ref} from "react";

type FieldProps = {
    label: string;
    value: string | null;
    onChange: (value: string) => void;
    inputRef?: Ref<HTMLInputElement>;
    rowClassName?: string;
    children?: ReactNode;
} & Omit<InputHTMLAttributes<HTMLInputElement>, "value" | "onChange" | "children">;

/* One labelled text field of a product form. Anything an <input> accepts passes
 * straight through, so a caller can ask for inputMode or a placeholder without
 * this component needing to know which field it is rendering. */
export const Field = ({label, value, onChange, inputRef, rowClassName, children, ...rest}: FieldProps) => (
    <div className={"productInput textBord" + (rowClassName ? " " + rowClassName : "")}>
        <p>{label}</p>
        {/* value ?? "" lives here rather than at each caller: a field that starts
            null and later holds a string makes React swap the input from
            uncontrolled to controlled, which drops the first character typed */}
        <input {...rest}
               className="iField"
               type="text"
               spellCheck={false}
               value={value ?? ""}
               onChange={e => onChange(e.target.value)}
               ref={inputRef}/>
        {children}
    </div>
);
