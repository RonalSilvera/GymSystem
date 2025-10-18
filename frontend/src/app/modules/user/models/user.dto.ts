export interface UserDTO {
  userId?: string;
  name?: string | null;
  email?: string | null;
  role?: string | null;
  password?: string | null;
  profileImageUrl?: string | null;
  base64Image?: string | null;
}

export class User implements UserDTO {
  userId?: string;
  name?: string | null = null;
  email?: string | null = null;
  role?: string | null = null;
  password?: string | null = null;
  profileImageUrl?: string | null = null;
  base64Image?: string | null = null;
}
