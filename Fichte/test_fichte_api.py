#!/usr/bin/env python3
"""
Complete API test script for Fichte application
Tests all endpoints: registration, login, users, groups, messages
"""

import requests
import json
import time
import random
import string

class FichteAPITester:
    def __init__(self, base_url="http://localhost:5084"):
        self.base_url = base_url
        self.jwt_token = None
        self.user_id = None
        self.group_id = None
        self.group_invite_code = None
        self.session = requests.Session()
        
        # Second user for invite testing
        self.user2_jwt_token = None
        self.user2_id = None
        self.user2_session = requests.Session()
        
    def generate_random_string(self, length=8):
        return ''.join(random.choices(string.ascii_lowercase + string.digits, k=length))
    
    def print_response(self, response, endpoint_name):
        print(f"\n{'='*50}")
        print(f"Testing: {endpoint_name}")
        print(f"Status Code: {response.status_code}")
        print(f"Response: {response.text}")
        if response.headers.get('content-type', '').startswith('application/json'):
            try:
                json_data = response.json()
                print(f"JSON: {json.dumps(json_data, indent=2)}")
            except:
                pass
        print(f"{'='*50}")
        return response
    
    def test_register(self):
        """Test user registration"""
        username = f"testuser_{self.generate_random_string()}"
        password = "testpass123"
        
        data = {
            "Username": username,
            "Password": password
        }
        
        response = self.session.post(
            f"{self.base_url}/api/Auth/Register",
            json=data,
            headers={"Content-Type": "application/json"}
        )
        
        self.print_response(response, "User Registration")
        
        if response.status_code == 200:
            self.username = username
            self.password = password
            print(f"PASS Registration successful for user: {username}")
            return True
        else:
            print(f"FAIL Registration failed")
            return False
    
    def test_login(self):
        """Test user login and store JWT token"""
        data = {
            "Username": self.username,
            "Password": self.password
        }
        
        response = self.session.post(
            f"{self.base_url}/api/Auth/Login",
            json=data,
            headers={"Content-Type": "application/json"}
        )
        
        self.print_response(response, "User Login")
        
        if response.status_code == 200:
            self.jwt_token = response.text.strip('"')
            self.session.headers.update({"Authorization": f"Bearer {self.jwt_token}"})
            print(f"PASS Login successful, JWT token stored")
            return True
        else:
            print(f"FAIL Login failed")
            return False
    
    def test_get_current_user(self):
        """Test getting current user info"""
        response = self.session.get(f"{self.base_url}/api/Users/me")
        self.print_response(response, "Get Current User")
        
        if response.status_code == 200:
            try:
                user_data = response.json()
                self.user_id = user_data.get('id')
                print(f"PASS Current user retrieved, ID: {self.user_id}")
                return True
            except:
                pass
        
        print(f"FAIL Failed to get current user")
        return False
    
    def test_get_online_users(self):
        """Test getting online users"""
        response = self.session.get(f"{self.base_url}/api/Users/online")
        self.print_response(response, "Get Online Users")
        
        if response.status_code == 200:
            print(f"PASS Online users retrieved")
            return True
        else:
            print(f"FAIL Failed to get online users")
            return False
    
    def test_create_group(self):
        """Test creating a group"""
        data = {
            "Name": f"Test Group {self.generate_random_string()}",
            "Description": "A test group created by API tester",
            "MaxMembers": 10
        }
        
        response = self.session.post(
            f"{self.base_url}/api/Group/CreateGroup",
            json=data,
            headers={"Content-Type": "application/json"}
        )
        
        self.print_response(response, "Create Group")
        
        if response.status_code == 200:
            try:
                group_data = response.json()
                self.group_id = group_data.get('id')
                self.group_invite_code = group_data.get('inviteCode')
                print(f"PASS Group created successfully, ID: {self.group_id}, Invite Code: {self.group_invite_code}")
                return True
            except:
                pass
        
        print(f"FAIL Failed to create group")
        return False
    
    def test_get_user_groups(self):
        """Test getting user's groups"""
        response = self.session.get(f"{self.base_url}/api/Group/GetUserGroups")
        self.print_response(response, "Get User Groups")
        
        if response.status_code == 200:
            print(f"PASS User groups retrieved")
            return True
        else:
            print(f"FAIL Failed to get user groups")
            return False
    
    def test_send_message_to_group(self):
        """Test sending a message to a group"""
        if not self.group_id:
            print("FAIL No group ID available for sending message")
            return False
        
        data = {
            "Body": f"Hello from API tester! Random: {self.generate_random_string()}",
            "GroupID": self.group_id
        }
        
        response = self.session.post(
            f"{self.base_url}/api/Messages/SendMessage",
            json=data,
            headers={"Content-Type": "application/json"}
        )
        
        self.print_response(response, "Send Message to Group")
        
        if response.status_code == 200:
            print(f"PASS Message sent to group successfully")
            return True
        else:
            print(f"FAIL Failed to send message to group")
            return False
    
    def test_get_user_messages(self):
        """Test getting user messages"""
        # Get all messages
        response = self.session.get(f"{self.base_url}/api/Messages/GetUserMessages")
        self.print_response(response, "Get All User Messages")
        
        # Get group messages if we have a group
        if self.group_id:
            response = self.session.get(f"{self.base_url}/api/Messages/GetUserMessages?groupId={self.group_id}")
            self.print_response(response, f"Get Group Messages (Group ID: {self.group_id})")
        
        if response.status_code == 200:
            print(f"PASS User messages retrieved")
            return True
        else:
            print(f"FAIL Failed to get user messages")
            return False
    
    def test_search_messages(self):
        """Test searching messages"""
        response = self.session.get(f"{self.base_url}/api/Messages/SearchMessages?query=Hello")
        self.print_response(response, "Search Messages")
        
        if response.status_code == 200:
            print(f"PASS Message search completed")
            return True
        else:
            print(f"FAIL Failed to search messages")
            return False
    
    def test_logout(self):
        """Test user logout"""
        if not self.user_id:
            print("FAIL No user ID available for logout")
            return False
        
        response = self.session.post(f"{self.base_url}/api/Auth/Logout?userId={self.user_id}")
        self.print_response(response, "User Logout")
        
        if response.status_code == 200:
            print(f"PASS User logged out successfully")
            return True
        else:
            print(f"FAIL Failed to logout user")
            return False
    
    def test_register_second_user(self):
        """Test registration of second user for invite testing"""
        username = f"testuser2_{self.generate_random_string()}"
        password = "testpass123"
        
        data = {
            "Username": username,
            "Password": password
        }
        
        response = self.user2_session.post(
            f"{self.base_url}/api/Auth/Register",
            json=data,
            headers={"Content-Type": "application/json"}
        )
        
        self.print_response(response, "Second User Registration")
        
        if response.status_code == 200:
            self.username2 = username
            self.password2 = password
            print(f"PASS Second user registration successful: {username}")
            return True
        else:
            print(f"FAIL Second user registration failed")
            return False
    
    def test_login_second_user(self):
        """Test login of second user"""
        data = {
            "Username": self.username2,
            "Password": self.password2
        }
        
        response = self.user2_session.post(
            f"{self.base_url}/api/Auth/Login",
            json=data,
            headers={"Content-Type": "application/json"}
        )
        
        self.print_response(response, "Second User Login")
        
        if response.status_code == 200:
            self.user2_jwt_token = response.text.strip('"')
            self.user2_session.headers.update({"Authorization": f"Bearer {self.user2_jwt_token}"})
            print(f"PASS Second user login successful, JWT token stored")
            return True
        else:
            print(f"FAIL Second user login failed")
            return False
    
    def test_get_second_user_info(self):
        """Test getting second user info"""
        response = self.user2_session.get(f"{self.base_url}/api/Users/me")
        self.print_response(response, "Get Second User Info")
        
        if response.status_code == 200:
            try:
                user_data = response.json()
                self.user2_id = user_data.get('id')
                print(f"PASS Second user info retrieved, ID: {self.user2_id}")
                return True
            except:
                pass
        
        print(f"FAIL Failed to get second user info")
        return False
    
    def test_join_group_with_invite(self):
        """Test second user joining group with invite code"""
        if not self.group_invite_code:
            print("FAIL No invite code available for joining group")
            return False
        
        response = self.user2_session.post(
            f"{self.base_url}/api/Group/JoinGroup?inviteCode={self.group_invite_code}",
            headers={"Content-Type": "application/json"}
        )
        
        self.print_response(response, f"Join Group with Invite Code: {self.group_invite_code}")
        
        if response.status_code == 200:
            print(f"PASS Second user successfully joined group with invite code")
            return True
        else:
            print(f"FAIL Second user failed to join group with invite code")
            return False
    
    def test_second_user_get_groups(self):
        """Test second user getting their groups (should include the joined group)"""
        response = self.user2_session.get(f"{self.base_url}/api/Group/GetUserGroups")
        self.print_response(response, "Second User Get Groups")
        
        if response.status_code == 200:
            try:
                groups = response.json()
                joined_group = next((g for g in groups if g.get('id') == self.group_id), None)
                if joined_group:
                    print(f"PASS Second user successfully has joined group in their groups list")
                    return True
                else:
                    print(f"FAIL Joined group not found in second user's groups list")
                    return False
            except:
                pass
        
        print(f"FAIL Failed to get second user's groups")
        return False
    
    def test_second_user_send_message_to_group(self):
        """Test second user sending message to joined group"""
        if not self.group_id:
            print("FAIL No group ID available for sending message")
            return False
        
        data = {
            "Body": f"Hello from second user! Random: {self.generate_random_string()}",
            "GroupID": self.group_id
        }
        
        response = self.user2_session.post(
            f"{self.base_url}/api/Messages/SendMessage",
            json=data,
            headers={"Content-Type": "application/json"}
        )
        
        self.print_response(response, "Second User Send Message to Group")
        
        if response.status_code == 200:
            print(f"PASS Second user successfully sent message to joined group")
            return True
        else:
            print(f"FAIL Second user failed to send message to group")
            return False
    
    def run_all_tests(self):
        """Run all API tests in sequence"""
        print("Starting Fichte API Tests")
        print(f"Base URL: {self.base_url}")
        
        tests = [
            ("Registration", self.test_register),
            ("Login", self.test_login),
            ("Get Current User", self.test_get_current_user),
            ("Get Online Users", self.test_get_online_users),
            ("Create Group", self.test_create_group),
            ("Get User Groups", self.test_get_user_groups),
            ("Send Message to Group", self.test_send_message_to_group),
            ("Get User Messages", self.test_get_user_messages),
            ("Search Messages", self.test_search_messages),
            ("Register Second User", self.test_register_second_user),
            ("Login Second User", self.test_login_second_user),
            ("Get Second User Info", self.test_get_second_user_info),
            ("Join Group with Invite", self.test_join_group_with_invite),
            ("Second User Get Groups", self.test_second_user_get_groups),
            ("Second User Send Message", self.test_second_user_send_message_to_group),
            ("Logout", self.test_logout)
        ]
        
        results = []
        for test_name, test_func in tests:
            try:
                success = test_func()
                results.append((test_name, success))
                time.sleep(0.5)  # Small delay between tests
            except Exception as e:
                print(f"FAIL {test_name} failed with exception: {e}")
                results.append((test_name, False))
        
        # Summary
        print(f"\n{'='*60}")
        print("TEST SUMMARY")
        print(f"{'='*60}")
        
        passed = 0
        for test_name, success in results:
            status = "PASS" if success else "FAIL"
            print(f"{test_name}: {status}")
            if success:
                passed += 1
        
        print(f"\nTotal: {len(results)}, Passed: {passed}, Failed: {len(results) - passed}")
        print(f"Success Rate: {passed/len(results)*100:.1f}%")

if __name__ == "__main__":
    tester = FichteAPITester()
    tester.run_all_tests()