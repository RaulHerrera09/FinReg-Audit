import { NavLink, useNavigate } from 'react-router-dom'
import { AlertTriangle, Building2, LayoutDashboard, LogOut, ScanLine } from 'lucide-react'
import { useAuthStore } from '../../store/authStore'

const navItems = [
  { path: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { path: '/accounts', label: 'Accounts', icon: Building2 },
  { path: '/alerts', label: 'Alerts', icon: AlertTriangle },
]

export default function Sidebar() {
  const logout = useAuthStore((s) => s.logout)
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <aside className="sidebar">
      <div className="brand-block">
        <div className="flex items-center gap-3">
          <ScanLine size={24} aria-hidden="true" />
          <div>
            <p className="brand-name">FinReg Audit</p>
            <p className="brand-kicker">Control room</p>
          </div>
        </div>
      </div>

      <nav className="sidebar-nav" aria-label="Primary navigation">
        {navItems.map(({ path, label, icon: Icon }) => (
          <NavLink
            key={path}
            to={path}
            className={({ isActive }) =>
              `nav-item ${
                isActive
                  ? 'nav-item-active'
                  : ''
              }`
            }
          >
            <Icon size={18} />
            {label}
          </NavLink>
        ))}
      </nav>

      <div className="sidebar-footer">
        <button
          onClick={handleLogout}
          className="nav-item signout"
        >
          <LogOut size={18} />
          Sign out
        </button>
      </div>
    </aside>
  )
}
