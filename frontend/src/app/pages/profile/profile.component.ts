import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DxFormModule } from 'devextreme-angular/ui/form';
import { DxButtonModule } from 'devextreme-angular/ui/button';
import { DxFileUploaderModule } from 'devextreme-angular/ui/file-uploader';
import { UserService } from '../../modules/user/services/user.service';
import { AuthService } from '../../core/services/auth.service';
import { UserDTO, User } from '../../modules/user/models/user.dto';
import { environment } from '../../../environments/environment';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, DxFormModule, DxButtonModule, DxFileUploaderModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
  user: UserDTO = new User();
  defaultImage = 'assets/img/default-avatar.png';
  pendingBase64?: string;
  private currentUserId?: string;
  private readonly apiBase = environment.API_URL.replace('api/', '');

  // Responsive breakpoints for DevExtreme form
  screenByWidth = (width: number) => {
    if (width < 768) return 'xs';
    if (width < 992) return 'sm';
    if (width < 1200) return 'md';
    return 'lg';
  };

  saveButtonOptions = {
    text: 'Guardar',
    type: 'success',
    icon: 'fa fa-check',
    useSubmitBehavior: true,
    width: '120px'
  };

  constructor(private userService: UserService, private auth: AuthService) {}

  ngOnInit(): void {
    const token = this.auth.getToken();
    if (!token) {
      return;
    }

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const idClaim = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
      const emailClaim = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';
      const roleClaim = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

      this.currentUserId = payload[idClaim] || payload['sub'];
      this.user.email = payload[emailClaim];
      this.user.role = payload[roleClaim];

      if (this.currentUserId) {
        this.userService.getById(this.currentUserId).subscribe(u => {
          this.user = u;
          this.auth.updateAvatar(this.getImageUrl(this.user.profileImageUrl) || this.defaultImage);
        });
      }
    } catch (e) {
      console.error('Invalid token');
    }
  }

  onFileChanged(e: any): void {
    const file = e.value?.[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = () => {
        this.pendingBase64 = reader.result as string;
        this.auth.updateAvatar(this.pendingBase64);
      };
      reader.readAsDataURL(file);
    }
  }

  save(): void {
    if (!this.currentUserId) {
      return;
    }

    const payload: UserDTO = {
      name: this.user.name,
      email: this.user.email,
      role: this.user.role,
      password: this.user.password,
    };
    
    if (this.pendingBase64) {
      payload.base64Image = this.pendingBase64.split(',')[1];
    }


    this.userService.update(this.currentUserId, payload).subscribe(res => {
      this.user = res;
      const avatar = this.pendingBase64 || this.getImageUrl(this.user.profileImageUrl) || this.defaultImage;
      this.auth.updateAvatar(avatar);
      this.pendingBase64 = undefined;
      window.location.reload();
    });
  }

  clearForm(): void {
    if (!this.currentUserId) return;
    
    // Reload original user data
    this.userService.getById(this.currentUserId).subscribe(u => {
      this.user = u;
      this.pendingBase64 = undefined;
      this.auth.updateAvatar(this.getImageUrl(this.user.profileImageUrl) || this.defaultImage);
    });
  }

  getImageUrl(fileName?: string | null): string | null {
    return fileName ? `${this.apiBase}profile-images/${fileName}` : null;
  }
}