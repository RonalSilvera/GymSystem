export interface FormDTO {
  formId?: string;
  formType?: string | null;
  formTypeId?: string;
  formGroupId?: string;
  name?: string | null;
  contactEmail?: string | null;
  contactPhone?: string | null;
  address?: string | null;
  version?: number;
}

export class Form implements FormDTO {
  formId?: string;
  formType?: string | null = null;
  formTypeId?: string;
  formGroupId?: string;
  name?: string | null = null;
  contactEmail?: string | null = null;
  contactPhone?: string | null = null;
  address?: string | null = null;
  version?: number;
}
