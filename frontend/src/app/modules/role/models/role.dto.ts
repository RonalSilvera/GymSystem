export interface RoleDTO {
  roleId?: string;
  name?: string | null;
  description?: string | null;
}

export class Role implements RoleDTO {
  roleId?: string;
  name?: string | null = null;
  description?: string | null = null;
}
